using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Item
{
    public class ItemNudge : MonoBehaviour
    {
        private WaitForSeconds _pause;
        private bool _isAnimating = false;

        private void Awake()
        {
            _pause = new WaitForSeconds(0.04f);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_isAnimating) return;
            
            if (gameObject.transform.position.x < collision.transform.position.x)
            {
                StartCoroutine(RotateAntiClock());
            }
            else
            {
                StartCoroutine(RotateClock());
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (_isAnimating) return;
            
            if (gameObject.transform.position.x > collision.transform.position.x)
            {
                StartCoroutine(RotateAntiClock());
            }
            else
            {
                StartCoroutine(RotateClock());
            }
        }

        private IEnumerator RotateAntiClock()
        {
            _isAnimating = true;

            for (int i = 0; i < 4; i++)
            {
                gameObject.transform.GetChild(0).Rotate(0f, 0f, 2f);
                yield return _pause;
            }

            for (int i = 0; i < 5; i++)
            {
                gameObject.transform.GetChild(0).Rotate(0f, 0f, -2f);
                yield return _pause;
            }

            gameObject.transform.GetChild(0).Rotate(0f, 0f, 2f);

            yield return _pause;
            _isAnimating = false;
        }

        private IEnumerator RotateClock()
        {
            _isAnimating = true;

            for (int i = 0; i < 4; i++)
            {
                gameObject.transform.GetChild(0).Rotate(0f, 0f, -2f);
                yield return _pause;
            }

            for (int i = 0; i < 5; i++)
            {
                gameObject.transform.GetChild(0).Rotate(0f, 0f, 2f);
                yield return _pause;
            }

            gameObject.transform.GetChild(0).Rotate(0f, 0f, -2f);

            yield return _pause;
            _isAnimating = false;
        }
    }
}