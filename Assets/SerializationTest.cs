
namespace IndentedDialogue
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class SerializationTest : MonoBehaviour
    {
        public string idpFilePath;
        public string binaryFilePath;

        public DialogueForest forest;

        [ContextMenu("Parse")]
        void Parse()
        {
            forest = new DialogueForest();
            forest.ParseFromFile(idpFilePath);

            Debug.Log("Parsed successfully");
        }

        [ContextMenu("Serialize")]
        void Serialize()
        {
            forest.SerializeIntoBinary(binaryFilePath);

            Debug.Log("Serialized successfully");
        }

        [ContextMenu("Deserialize")]
        void Deserialize()
        {
            forest = DialogueForest.DeserializeFromBinary(binaryFilePath);

            Debug.Log("Deserialized successfully");
        }
    }
}