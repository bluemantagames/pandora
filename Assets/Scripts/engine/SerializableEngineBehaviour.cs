using System;

namespace Pandora.Engine 
{
  [Serializable]
  public class SerializableEngineBehaviour
  {
    public string ComponentName;

    public SerializableEngineBehaviour(string componentName)
    {
      ComponentName = componentName;
    }
  }
}