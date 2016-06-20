using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ES2UserType_ES2GlobalSettings : ES2Type
{
	public override void Write(object obj, ES2Writer writer)
	{
		ES2GlobalSettings data = (ES2GlobalSettings)obj;
		// Add your writer.Write calls here.
writer.Write(data.saveLocation);writer.Write(data.optimizeMode);writer.Write(data.encrypt);writer.Write(data.encryptionType);writer.Write(data.bufferSize);writer.Write(data.useGUILayout);
writer.Write(data.enabled);
writer.Write(data.hideFlags);

	}
	
	public override object Read(ES2Reader reader)
	{
		ES2GlobalSettings data = GetOrCreate<ES2GlobalSettings>();
		Read(reader, data);
		return data;
	}

	public override void Read(ES2Reader reader, object c)
	{
		ES2GlobalSettings data = (ES2GlobalSettings)c;
		// Add your reader.Read calls here to read the data into the object.
data.saveLocation = reader.Read<ES2Settings.SaveLocation>();
data.optimizeMode = reader.Read<ES2Settings.OptimizeMode>();
data.encrypt = reader.Read<System.Boolean>();
data.encryptionType = reader.Read<ES2Settings.EncryptionType>();
data.bufferSize = reader.Read<System.Int32>();
data.useGUILayout = reader.Read<System.Boolean>();
data.enabled = reader.Read<System.Boolean>();
data.hideFlags = reader.Read<UnityEngine.HideFlags>();

	}
	
	/* ! Don't modify anything below this line ! */
	public ES2UserType_ES2GlobalSettings():base(typeof(ES2GlobalSettings)){}
}