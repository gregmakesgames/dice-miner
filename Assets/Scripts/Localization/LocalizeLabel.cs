using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceMiner.Localization
{
    public class LocalizeLabel : MonoBehaviour
    {
        [SerializeField] private string key;

        private Text _label;
        private TMP_Text _tmplabel ;

        private void Start()
        {
            InitLabel();
            SetString(L.Get(key));
        }

        private void InitLabel()
        {
            if (_label == null)
            {
                _label = GetComponent<Text>();
            }
            if (_tmplabel == null)
            {
                _tmplabel = GetComponent<TMP_Text>();
            }
        }

        private void SetString(string value)
        {
            if (_label != null) _label.text = value;
            if (_tmplabel != null) _tmplabel.text = value;
        }
    }
}