

using Tools;

namespace Actors.Models;

public class ActorMetadata
{
    [JsonPropertyName("id")]
    public string ID { get; set; }
    [JsonPropertyName("actorRemindersMetadata")]
    public ActorRemindersMetadata RemindersMetadata { get; set; } = new();
    [JsonIgnore]
    public string Etag { get; set; }

    public uint CalculateReminderPartition(string actorID, string reminderName)
    {
        if (RemindersMetadata.PartitionCount <= 0)
            return 0;

        uint hash = HashTool.Fnv1aHash32(actorID + reminderName);
        return (hash % (uint)RemindersMetadata.PartitionCount) + 1;
    }
    public ActorReminderReference CreateReminderReference(Reminder reminder)
    {
        if (RemindersMetadata.PartitionCount > 0)
        {
            return new ActorReminderReference
            {
                ActorMetadataID = ID,
                ActorRemindersPartitionID = CalculateReminderPartition(reminder.ActorID, reminder.Name),
                Reminder = reminder,
            };
        }

        return new ActorReminderReference
        {
            ActorMetadataID = Guid.NewGuid().ToString(),
            ActorRemindersPartitionID = 0,
            Reminder = reminder,
        };
    }

    public string CalculateRemindersStateKey(string actorType, uint remindersPartitionID)
    {
        if (remindersPartitionID == 0)
        {
            return "actors||" + actorType;
        }

        return string.Join("||", "actors", actorType, ID, "reminders", remindersPartitionID);
    }

    public string CalculateEtag(uint partitionID)
    {
        return RemindersMetadata.PartitionsEtag[partitionID];
    }

    public (bool, List<Reminder>, string, string) RemoveReminderFromPartition(List<ActorReminderReference> reminderRefs, string actorType, string actorID, string reminderName)
    {
        uint partitionID = 0;
        var l = reminderRefs.Count;
        bool found = false;
        if (RemindersMetadata.PartitionCount > 0)
        {
            foreach (var reminderRef in reminderRefs)
            {
                if (reminderRef.Reminder.ActorType == actorType && reminderRef.Reminder.ActorID == actorID && reminderRef.Reminder.Name == reminderName)
                {
                    partitionID = reminderRef.ActorRemindersPartitionID;
                    found = true;
                    break;
                }
            }

            // If the reminder doesn't exist, return without making any change
            if (!found)
            {
                return (false, [], "", "");
            }

            l /= RemindersMetadata.PartitionCount;
        }

        var remindersInPartitionAfterRemoval = new List<Reminder>();
        found = false;
        foreach (var reminderRef in reminderRefs)
        {
            if (reminderRef.Reminder.ActorType == actorType && reminderRef.Reminder.ActorID == actorID && reminderRef.Reminder.Name == reminderName)
            {
                found = true;
                continue;
            }
            if (reminderRef.ActorRemindersPartitionID == partitionID)
            {
                remindersInPartitionAfterRemoval.Add(reminderRef.Reminder);
            }
        }

        if (!found)
        {
            return (false, [], "", "");
        }

        var stateKey = CalculateRemindersStateKey(actorType, partitionID);
        return (true, remindersInPartitionAfterRemoval, stateKey, CalculateEtag(partitionID));
    }

    public (List<Reminder>, ActorReminderReference, string, string) InsertReminderInPartition(List<ActorReminderReference> reminderRefs, Reminder reminder)
    {
        var newReminderRef = CreateReminderReference(reminder);

        var remindersInPartitionAfterInsertion = new List<Reminder>();
        foreach (var reminderRef in reminderRefs)
        {
            if (reminderRef.ActorRemindersPartitionID == newReminderRef.ActorRemindersPartitionID)
            {
                remindersInPartitionAfterInsertion.Add(reminderRef.Reminder);
            }
        }

        remindersInPartitionAfterInsertion.Add(reminder);
        var stateKey = CalculateRemindersStateKey(newReminderRef.Reminder.ActorType, newReminderRef.ActorRemindersPartitionID);
        return (remindersInPartitionAfterInsertion, newReminderRef, stateKey, CalculateEtag(newReminderRef.ActorRemindersPartitionID));
    }

    public string CalculateDatabasePartitionKey(string stateKey)
    {
        if (RemindersMetadata.PartitionCount > 0)
        {
            return ID;
        }

        return stateKey;
    }
}

public class ActorRemindersMetadata
{
    [JsonPropertyName("partitionCount")]
    public int PartitionCount { get; set; }
    [JsonIgnore]
    public Dictionary<uint, string> PartitionsEtag { get; set; } = [];
}

public class ActorReminderReference
{
    public string ActorMetadataID { get; set; }
    public uint ActorRemindersPartitionID { get; set; }
    public Reminder Reminder { get; set; }
}
