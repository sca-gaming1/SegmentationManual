namespace SegmentationManual.Models;

public static class LifecycleStageMapping
{
    private static readonly Dictionary<string, string> _stageNameToId = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Historical", "1" },
        { "Non Depositors", "2" },
        { "Dormant", "3" },
        { "Churn 2", "4" },
        { "Churn 1", "5" },
        { "Active", "6" },
        { "First Depositors", "7" },
        { "Fun", "8" },
        { "Unknown", "-1" }
    };

    private static readonly Dictionary<string, string> _stageIdToName = new()
    {
        { "1", "Historical" },
        { "2", "Non Depositors" },
        { "3", "Dormant" },
        { "4", "Churn 2" },
        { "5", "Churn 1" },
        { "6", "Active" },
        { "7", "First Depositors" },
        { "8", "Fun" },
        { "-1", "Unknown" }
    };

    public static string? GetStageId(string stageName)
    {
        if (_stageNameToId.TryGetValue(stageName, out var id))
        {
            return id;
        }
        return null;
    }

    public static string? GetStageName(string stageId)
    {
        if (_stageIdToName.TryGetValue(stageId, out var name))
        {
            return name;
        }
        return null;
    }

    public static bool TryGetStageId(string stageName, out string stageId)
    {
        if (_stageNameToId.TryGetValue(stageName, out var foundId))
        {
            stageId = foundId;
            return true;
        }
        stageId = string.Empty;
        return false;
    }
}

public static class VipStatusMapping
{
    private static readonly Dictionary<string, string> _clubLevelToGuid = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Copper", "651c5e91-7a6d-45d4-957a-78f75389b398" },
        { "Diamond", "91276247-b9bd-43d2-8138-50c88154e6f0" },
        { "Silver", "216df65f-22c9-4431-83b0-5a8b8774c4fe" },
        { "Gold", "0571c538-da08-4ada-822a-614bc86b6866" },
        { "Diamond**", "c8bbea61-a3ba-4e38-9148-5696909e34cd" },
        { "Platinum", "42099eb9-7c7b-427e-a664-5b74f4f7bae2" },
        { "Bronze", "f8daea4e-0796-4c8f-ba79-b4bd3074a5a6" },
        { "Diamond*", "af6cb862-a4ae-4af4-a220-1a2f5fcf9751" },
        { "Blue", "53f7799a-83ac-4f90-be2a-b586efa8cd51" },
        { "Diamond***", "840bc786-d58b-4cbf-8267-12dc5025dc15" },
        { "Diamond****", "bf0f8a90-328d-4ceb-8b0d-5f7e28b036cc" }
    };

    private static readonly Dictionary<string, string> _guidToClubLevel = new(StringComparer.OrdinalIgnoreCase)
    {
        { "651c5e91-7a6d-45d4-957a-78f75389b398", "Copper" },
        { "91276247-b9bd-43d2-8138-50c88154e6f0", "Diamond" },
        { "216df65f-22c9-4431-83b0-5a8b8774c4fe", "Silver" },
        { "0571c538-da08-4ada-822a-614bc86b6866", "Gold" },
        { "c8bbea61-a3ba-4e38-9148-5696909e34cd", "Diamond**" },
        { "42099eb9-7c7b-427e-a664-5b74f4f7bae2", "Platinum" },
        { "f8daea4e-0796-4c8f-ba79-b4bd3074a5a6", "Bronze" },
        { "af6cb862-a4ae-4af4-a220-1a2f5fcf9751", "Diamond*" },
        { "53f7799a-83ac-4f90-be2a-b586efa8cd51", "Blue" },
        { "840bc786-d58b-4cbf-8267-12dc5025dc15", "Diamond***" },
        { "bf0f8a90-328d-4ceb-8b0d-5f7e28b036cc", "Diamond****" }
    };

    public static string? GetClubLevelGuid(string clubLevel)
    {
        if (_clubLevelToGuid.TryGetValue(clubLevel, out var guid))
        {
            return guid;
        }
        return null;
    }

    public static string? GetClubLevel(string guid)
    {
        if (_guidToClubLevel.TryGetValue(guid, out var level))
        {
            return level;
        }
        return null;
    }

    public static bool TryGetClubLevelGuid(string clubLevel, out string guid)
    {
        if (_clubLevelToGuid.TryGetValue(clubLevel, out var foundGuid))
        {
            guid = foundGuid;
            return true;
        }
        guid = string.Empty;
        return false;
    }
}
