namespace OpenCashFlow.Contracts.Core;

public class Core_Timezone
{
    public Guid TimezoneID { get; set; }
    public string? TimezoneName { get; set; }
    public string? TimezoneValue { get; set; }
    public bool Visible { get; set; } = true;
    public int DisplayOrder { get; set; } = 999;
}

public class Core_Country_LookUp_Type
{
    public Guid CountryID { get; set; }
    public string? CountryName { get; set; }
    public string? CountryCode { get; set; }
    public bool Visible { get; set; } = true;
    public int DisplayOrder { get; set; } = 999;
}

public class Core_Contacts_Email_LookUp_Type
{
    public Guid EmailTypeID { get; set; }
    public string? EmailTypeName { get; set; }
    public string? EmailTypeDescription { get; set; }
    public bool Visible { get; set; } = true;
    public int DisplayOrder { get; set; } = 999;
}

public class Core_Contacts_Phone_LookUp_Type
{
    public Guid PhoneTypeID { get; set; }
    public string? PhoneTypeName { get; set; }
    public string? PhoneTypeDescription { get; set; }
    public bool Visible { get; set; } = true;
    public int DisplayOrder { get; set; } = 999;
}
