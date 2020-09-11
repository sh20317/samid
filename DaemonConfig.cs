using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace systemdsensordaemon
{
  public class DaemonConfig
  {
    public bool in_use;
    public String outputServer;
    public int pollingFrequency;
    public List<Source> sources;
    public bool isLogging;
  }

  public class Source {
    public bool in_use;
    public int howManySecondsBack;
    public int take;
    public bool doNotReadIfDuplicate;
    public bool deleteOnRead;
    public String sourceUri;
    public String type;
    public String url;
    public List<ColumnItem> inputColumns;
  }

  public class ColumnItem {
    public String columnName;
    public String columnNameConvert;    
    public String type;
    public String typeConvert;
    public String value;
  }
}
