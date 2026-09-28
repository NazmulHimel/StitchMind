using GarmentsERP.Shared.Domain;
using GarmentsERP.HR.Domain.Enums;

namespace GarmentsERP.HR.Domain.Entities;

public class BiometricDevice : BaseEntity
{
    public string DeviceCode { get; set; } = string.Empty; // e.g. BIO-GATE-01
    public string DeviceName { get; set; } = string.Empty; // e.g. Main Gate ZKTeco BioTime
    public string IpAddress { get; set; } = "192.168.10.201";
    public int PortNumber { get; set; } = 4370; // Standard ZKTeco TCP/IP port
    public BiometricProtocolType ProtocolType { get; set; } = BiometricProtocolType.ZKTeco_Standalone;
    public string PhysicalLocation { get; set; } = "Main Factory Entrance"; // Main Gate, Line 04, Canteen
    public bool IsOnline { get; set; } = true;
    public DateTime? LastSyncTime { get; set; }
    public int TotalPunchesRecorded { get; set; }

    public ICollection<BiometricPunchLog> PunchLogs { get; set; } = new List<BiometricPunchLog>();
}

public class BiometricPunchLog : BaseEntity
{
    public Guid BiometricDeviceId { get; set; }
    public BiometricDevice BiometricDevice { get; set; } = null!;

    public Guid? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public string BiometricBadgeNumber { get; set; } = string.Empty;
    public DateTime PunchTimestamp { get; set; }
    public PunchInOutMode PunchMode { get; set; } = PunchInOutMode.CheckIn;
    public string VerificationType { get; set; } = "Fingerprint"; // Fingerprint, FaceScan, RFID Card
    public bool IsProcessedToAttendance { get; set; } = true;
    public string? RawPayload { get; set; }
}
