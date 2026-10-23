using UnityEngine;
using UnityEngine.UI;

namespace Shiftworks.UI
{
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private Text promptText;

        public void Show(string text)
        {
            if (promptText == null)
            {
                return;
            }

            promptText.text = text;
            promptText.enabled = true;
        }

        public void Hide()
        {
            if (promptText == null)
            {
                return;
            }

            promptText.enabled = false;
        }
    }
}
