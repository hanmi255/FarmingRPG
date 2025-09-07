using UnityEngine;

namespace Assets.Scripts.Item
{
    public class TriggerObscuringItemFader : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            ObscuringItemFader[] obscuringItemFaders = collision.gameObject.GetComponentsInChildren<ObscuringItemFader>();

            foreach (ObscuringItemFader fader in obscuringItemFaders)
            {
                fader.FadeOut();
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            ObscuringItemFader[] obscuringItemFaders = collision.gameObject.GetComponentsInChildren<ObscuringItemFader>();

            foreach (ObscuringItemFader fader in obscuringItemFaders)
            {
                fader.FadeIn();
            }
        }
    }
}