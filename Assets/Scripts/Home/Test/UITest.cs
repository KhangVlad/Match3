
    using TMPro;
    using UnityEngine;

    public class UITest : MonoBehaviour
    {
        public TextMeshProUGUI text;

        public void SetText(int index)
        {
            this.text.text = index.ToString();
        }
        
        
    }
