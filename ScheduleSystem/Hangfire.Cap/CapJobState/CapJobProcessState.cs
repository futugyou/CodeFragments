﻿namespace Hangfire.Cap.CapJobState;

public class CapJobProcessState : IState
{
    /// <summary>
    /// Represents the name of the <i>Processing</i> state. This field is read-only.
    /// </summary>
    /// <remarks>
    /// The value of this field is <c>"Processing"</c>.
    /// </remarks>
    public static readonly string StateName = "Processing";

    public CapJobProcessState(string serverId, string workerId, DateTime startAt)
    {
        if (string.IsNullOrWhiteSpace(serverId))
            throw new ArgumentNullException(nameof(serverId));
        this.ServerId = serverId;
        this.StartedAt = startAt;
        this.WorkerId = workerId;
    }

    /// <summary>
    /// Gets a date/time when the current state instance was created.
    /// </summary>
    [JsonIgnore]
    public DateTime StartedAt { get; }

    /// <summary>
    /// Gets the <i>instance id</i> of an instance of the <see cref="T:Hangfire.Server.BackgroundProcessingServer" />
    /// class, whose <see cref="T:Hangfire.Server.Worker" /> background process started to process an
    /// <i>enqueued</i> background job.
    /// </summary>
    /// <value>Usually the string representation of a GUID value, may vary in future versions.</value>
    public string ServerId { get; }

    /// <summary>
    /// Gets the identifier of a <see cref="T:Hangfire.Server.Worker" /> that started to
    /// process an <i>enqueued</i> background job.
    /// </summary>
    public string WorkerId { get; }

    /// <inheritdoc />
    /// <remarks>
    /// Always equals to <see cref="F:Hangfire.States.ProcessingState.StateName" /> for the <see cref="T:Hangfire.States.ProcessingState" />.
    /// Please see the remarks section of the <see cref="P:Hangfire.States.IState.Name">IState.Name</see>
    /// article for the details.
    /// </remarks>
    [JsonIgnore]
    public string Name
    {
        get
        {
            return StateName;
        }
    }

    /// <inheritdoc />
    public string Reason { get; set; }

    /// <inheritdoc />
    /// <remarks>
    /// Always returns <see langword="true" /> for the <see cref="T:Hangfire.States.ProcessingState" />.
    /// Please refer to the <see cref="P:Hangfire.States.IState.IsFinal">IState.IsFinal</see> documentation
    /// for the details.
    /// </remarks>
    [JsonIgnore]
    public bool IsFinal
    {
        get
        {
            return false;
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// Always returns <see langword="false" /> for the <see cref="T:Hangfire.States.ProcessingState" />.
    /// Please see the description of this property in the
    /// <see cref="P:Hangfire.States.IState.IgnoreJobLoadException">IState.IgnoreJobLoadException</see>
    /// article.
    /// </remarks>
    [JsonIgnore]
    public bool IgnoreJobLoadException
    {
        get
        {
            return false;
        }
    }

    /// <inheritdoc />
    public Dictionary<string, string> SerializeData()
    {
        return new Dictionary<string, string>()
      {
        {
          "StartedAt",
          JobHelper.SerializeDateTime(this.StartedAt)
        },
        {
          "ServerId",
          this.ServerId
        },
        {
          "WorkerId",
          this.WorkerId
        }
      };
    }
}