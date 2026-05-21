using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UrlParser : MonoBehaviour
{

    public static string openNavid = "";

    //use for test purpose in the editor!
    public bool testInEditor = false;
    public SpecieType editorForceSpecie;
    public string editorForceNavid;

    void Awake() {
#if !UNITY_EDITOR && UNITY_WEBGL
        try{
			string url = Application.absoluteURL;
			
			const string sepChar = "?";
			int paramsSep = url.IndexOf (sepChar);

			string[] urlSplit = url.Split (new string[]{ sepChar }, System.StringSplitOptions.RemoveEmptyEntries);
			
            if(urlSplit.Length > 1) {
                string[] urlParams = urlSplit[1].Split(new string[] { "&" }, System.StringSplitOptions.RemoveEmptyEntries);
                print("Found " + urlParams.Length + " params");
                foreach (string param in urlParams) {
                    string[] tokens = param.Split(new string[] { "=" }, System.StringSplitOptions.RemoveEmptyEntries);
                    string key = tokens[0];
                    string value = tokens[1];
                    print("Key=" + key + " ; " + "value=" + value);
                    switch (key) {
                        case "specie":
                            GlobalVariables.specieType = System.Enum.Parse<SpecieType>(value[0].ToString().ToUpper() + value.Substring(1));
                            break;
                        case "id":
                            openNavid = value;
                            break;
                        default:
                            print("key not recognized; Key: " + key + "; Value: " + value);
                            break;
                    }
                }
            }
		}
		catch(System.Exception e){
            throw e;
		}
#elif UNITY_EDITOR
        if (testInEditor) {
            GlobalVariables.specieType = editorForceSpecie;
            openNavid = editorForceNavid;
        }
#endif

        if (!string.IsNullOrEmpty(openNavid)) {
            GetComponent<LoadScene>().GoLoadScene();
        }
    }
}
