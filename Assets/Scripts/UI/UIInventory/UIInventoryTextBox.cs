using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.UIInventory
{
    public struct TextParameters
    {
        public string textTop1;
        public string textTop2;
        public string textTop3;
        public string textBottom1;
        public string textBottom2;
        public string textBottom3;
    }

    public class UIInventoryTextBox : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textMeshTop1 = null;
        [SerializeField] private TextMeshProUGUI textMeshTop2 = null;
        [SerializeField] private TextMeshProUGUI textMeshTop3 = null;
        [SerializeField] private TextMeshProUGUI textMeshBottom1 = null;
        [SerializeField] private TextMeshProUGUI textMeshBottom2 = null;
        [SerializeField] private TextMeshProUGUI textMeshBottom3 = null;

        public void SetTextBoxText(TextParameters textParameters)
        {
            textMeshTop1.text = textParameters.textTop1;
            textMeshTop2.text = textParameters.textTop2;
            textMeshTop3.text = textParameters.textTop3;
            textMeshBottom1.text = textParameters.textBottom1;
            textMeshBottom2.text = textParameters.textBottom2;
            textMeshBottom3.text = textParameters.textBottom3;
        }
    }
}
