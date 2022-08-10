﻿using Hangfire;
using Hangfire.Common;
using Hangfire.Logging;
using Hangfire.States;
using Hangfire.Storage;

namespace HangfireCapDemo;

public class AutomaticRetrySetAttribute : JobFilterAttribute, IElectStateFilter, IApplyStateFilter
{
    /// <summary>
    /// Represents the default number of retry attempts. This field is read-only.
    /// </summary>
    /// <remarks>
    /// The value of this field is <c>10</c>.
    /// </remarks>
    public static readonly int DefaultRetryAttempts = 5;

    private static readonly Func<long, int> DefaultDelayInSecondsByAttemptFunc = attempt =>
    {
        var random = new Random();
        return (int)Math.Round(
            Math.Pow(attempt - 1, 4) + 15 + random.Next(30) * attempt);
    };

    private readonly ILog _logger = LogProvider.For<AutomaticRetrySetAttribute>();

    private readonly object _lockObject = new object();
    private int _attempts;
    private int[] _delaysInSeconds;
    private Func<long, int> _delayInSecondsByAttemptFunc;
    private AttemptsExceededAction _onAttemptsExceeded;
    private bool _logEvents;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutomaticRetrySetAttribute"/>
    /// class with <see cref="DefaultRetryAttempts"/> number.
    /// </summary>
    public AutomaticRetrySetAttribute()
    {
    }

    /// <summary>
    /// Gets or sets the maximum number of automatic retry attempts.设置次数
    /// </summary>
    /// <value>Any non-negative number.</value>
    /// <exception cref="ArgumentOutOfRangeException">The value in a set operation is less than zero.</exception>
    public int Attempts
    {
        get { lock (_lockObject) { return _attempts; } }
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), @"Attempts value must be equal or greater than zero.");
            }

            lock (_lockObject)
            {
                _attempts = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the delays between attempts.设置重试区间
    /// </summary>
    /// <value>An array of non-negative numbers.</value>
    /// <exception cref="ArgumentNullException">The value in a set operation is null.</exception>
    /// <exception cref="ArgumentException">The value contain one or more negative numbers.</exception>
    public int[] DelaysInSeconds
    {
        get { lock (_lockObject) { return _delaysInSeconds; } }
        set
        {
            if (value == null || value.Length == 0) throw new ArgumentNullException(nameof(value));
            if (value.Any(delay => delay < 0)) throw new ArgumentException($@"{nameof(DelaysInSeconds)} value must be an array of non-negative numbers.", nameof(value));

            lock (_lockObject) { _delaysInSeconds = value; }
        }
    }

    /// <summary>
    /// Gets or sets a function using to get a delay by an attempt number.
    /// </summary>
    /// <exception cref="ArgumentNullException">The value in a set operation is null.</exception>
    public Func<long, int> DelayInSecondsByAttemptFunc
    {
        get { lock (_lockObject) { return _delayInSecondsByAttemptFunc; } }
        set
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            lock (_lockObject) { _delayInSecondsByAttemptFunc = value; }
        }
    }

    /// <summary>
    /// Gets or sets a candidate state for a background job that 
    /// will be chosen when number of retry attempts exceeded.
    /// </summary>
    public AttemptsExceededAction OnAttemptsExceeded
    {
        get { lock (_lockObject) { return _onAttemptsExceeded; } }
        set { lock (_lockObject) { _onAttemptsExceeded = value; } }
    }

    /// <summary>
    /// Gets or sets whether to produce log messages on retry attempts.
    /// </summary>
    public bool LogEvents
    {
        get { lock (_lockObject) { return _logEvents; } }
        set { lock (_lockObject) { _logEvents = value; } }
    }

    /// <inheritdoc />
    public void OnStateElection(ElectStateContext context)
    {
        var jobdta = context.BackgroundJob.Job.Args.FirstOrDefault();
        if (!(jobdta is CapJobItem capjob))
        {
            return;
        }

        if (capjob.RetryTimes <= 0)
        {
            return;
        }

        //如果先执行失败的话 就直接失败
        var failedState = context.CandidateState as FailedState;
        if (failedState == null)
        {
            // This filter accepts only failed job state.
            return;
        }

        var retryTimesLimit = Attempts;
        var retryAttempt = context.GetJobParameter<int>("RetryCount") + 1;
        if (capjob.RetryTimes > 0)
        {
            //自定义设置了超时配置
            retryTimesLimit = capjob.RetryTimes;
        }
        if (retryAttempt <= retryTimesLimit)
        {
            ScheduleAgainLater(context, retryAttempt, retryTimesLimit, failedState);
            return;
        }

        try
        {
            var isCronJob = !string.IsNullOrEmpty(capjob.Cron);
            var jobKey = isCronJob ? ((!string.IsNullOrEmpty(capjob.RecurringJobIdentifier) ? capjob.RecurringJobIdentifier : capjob.JobName)) : context.BackgroundJob.Id;

            var hashKey = CodingUtil.MD5(jobKey + ".runtime");
            context.Transaction.RemoveHash(hashKey);
        }
        catch (Exception)
        {
            //ignore
        }

        if (retryAttempt > retryTimesLimit && OnAttemptsExceeded == AttemptsExceededAction.Delete)
        {
            TransitionToDeleted(context, failedState);
        }
        else
        {
            if (LogEvents)
            {
                _logger.ErrorException(
                    $"Failed to process the job '{context.BackgroundJob.Id}': an exception occurred.",
                    failedState.Exception);
            }
        }
    }


    /// <inheritdoc />
    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        var capjob = context.BackgroundJob.Job.Args.FirstOrDefault() as CapJobItem;
        if (capjob == null || capjob.RetryTimes <= 0)
        {
            return;
        }

        if (context.NewState is ScheduledState && context.NewState.Reason != null && context.NewState.Reason.StartsWith("Retry attempt"))
        {
            transaction.AddToSet("retries", context.BackgroundJob.Id);
        }
    }

    /// <inheritdoc />
    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.OldStateName == ScheduledState.StateName)
        {
            transaction.RemoveFromSet("retries", context.BackgroundJob.Id);
        }
    }

    /// <summary>
    /// Schedules the job to run again later. See <see cref="DelayInSecondsByAttemptFunc"/>.
    /// </summary>
    /// <param name="context">The state context.</param>
    /// <param name="retryAttempt">The count of retry attempts made so far.</param>
    /// <param name="totalLimit">max limit set</param>
    /// <param name="failedState">Object which contains details about the current failed state.</param>
    private void ScheduleAgainLater(ElectStateContext context, int retryAttempt, int totalLimit, FailedState failedState)
    {
        var capjob = context.BackgroundJob.Job.Args.FirstOrDefault() as CapJobItem;
        if (capjob == null || capjob.RetryTimes <= 0)
        {
            return;
        }

        context.SetJobParameter("RetryCount", retryAttempt);
        int delayInSeconds;

        var delayArr = _delaysInSeconds;
        if (!string.IsNullOrEmpty(capjob.RetryDelaysInSeconds))
        {
            var delayArrStr = capjob.RetryDelaysInSeconds.Replace("，", ",").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (delayArrStr.Any())
            {
                try
                {
                    var tempArr = delayArrStr.Select(int.Parse).ToArray();
                    if (tempArr.Any()) delayArr = tempArr;
                }
                catch (Exception)
                {
                    //ignore
                }

            }
        }

        if (delayArr != null)
        {
            delayInSeconds = retryAttempt <= delayArr.Length
                ? delayArr[retryAttempt - 1]
                : delayArr.Last();
        }
        else
        {
            delayInSeconds = _delayInSecondsByAttemptFunc(retryAttempt);
        }

        var delay = TimeSpan.FromSeconds(delayInSeconds);

        const int maxMessageLength = 50;
        var exceptionMessage = failedState.Exception.Message.Length > maxMessageLength
            ? failedState.Exception.Message.Substring(0, maxMessageLength - 1) + "…"
            : failedState.Exception.Message;

        // If attempt number is less than max attempts, we should
        // schedule the job to run again later.

        var reason = $"Delay {delayInSeconds}s Retry attempt {retryAttempt} of {totalLimit}: {exceptionMessage}";

        context.CandidateState = delay == TimeSpan.Zero
            ? (IState)new EnqueuedState { Reason = reason }
            : new ScheduledState(delay) { Reason = reason };

        if (LogEvents)
        {
            _logger.WarnException(
                $"Failed to process the job '{context.BackgroundJob.Id}': an exception occurred. Retry attempt {retryAttempt} of {totalLimit} will be performed in {delayInSeconds}s.",
                failedState.Exception);
        }
    }

    /// <summary>
    /// Transition the candidate state to the deleted state.
    /// </summary>
    /// <param name="context">The state context.</param>
    /// <param name="failedState">Object which contains details about the current failed state.</param>
    private void TransitionToDeleted(ElectStateContext context, FailedState failedState)
    {
        context.CandidateState = new DeletedState
        {
            Reason = Attempts > 0
                ? "Exceeded the maximum number of retry attempts."
                : "Retries were disabled for this job."
        };

        if (LogEvents)
        {
            _logger.WarnException(
                $"Failed to process the job '{context.BackgroundJob.Id}': an exception occured. Job was automatically deleted because the retry attempt count exceeded {Attempts}.",
                failedState.Exception);
        }
    }
}