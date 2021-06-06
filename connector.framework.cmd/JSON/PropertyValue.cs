/// <summary>
/// Made with https://app.quicktype.io/.
/// </summary>
namespace connector.framework.cmd
{
  public partial struct PropertyValue
  {
    public bool? Bool;
    public double? Double;
    public long? Integer;
    public string String;

    public static implicit operator PropertyValue(bool Bool) => new PropertyValue { Bool = Bool };
    public static implicit operator PropertyValue(double Double) => new PropertyValue { Double = Double };
    public static implicit operator PropertyValue(long Integer) => new PropertyValue { Integer = Integer };
    public static implicit operator PropertyValue(string String) => new PropertyValue { String = String };
    public bool IsNull => Bool == null && Double == null && Integer == null && String == null;
  }
}
