using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.UI.Scrollbar;

namespace ExtendedUI.Runtime
{
    public class ExtendedDropdown : TMP_Dropdown, IMoveHandler
    {
        public override void OnSubmit(BaseEventData eventData)
        {
            base.OnSubmit(eventData);
            if (_scrollbar != null) return;
            var m_Dropdown = GetComponentInChildren<TMP_Dropdown>();
            ScrollRect itemTemplate = m_Dropdown.GetComponentsInChildren<ScrollRect>().FirstOrDefault(x => x.transform.parent.gameObject.activeInHierarchy);
            _scrollbar = itemTemplate.verticalScrollbar;

            _items = itemTemplate.GetComponentsInChildren<DropdownItem>();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            var m_Dropdown = GetComponentInChildren<TMP_Dropdown>();
            ScrollRect itemTemplate = m_Dropdown.GetComponentsInChildren<ScrollRect>().FirstOrDefault(x => x.transform.parent.gameObject.activeInHierarchy);
            _scrollbar = itemTemplate.verticalScrollbar;

            _items = itemTemplate.GetComponentsInChildren<DropdownItem>();
        }

        private void Update()
        {
            if (IsExpanded && _items != null)
            {
                if (EventSystem.current.currentSelectedGameObject == _selectedGO) return;
                _selectedGO = EventSystem.current.currentSelectedGameObject;

                DropdownItem selectedItem = _selectedGO.GetComponent<DropdownItem>();
                if (selectedItem == null) return;

                int itemPosition = _items.ToList().IndexOf(selectedItem);

                var valuePosition = (float)itemPosition / (_items.Length - 1);
                var value = _scrollbar.direction == Direction.TopToBottom ? valuePosition : 1f - valuePosition;
                _scrollbar.value = Mathf.Max(.001f, value);
            }
        }

        private DropdownItem[] _items;
        private GameObject _selectedGO;
        private Scrollbar _scrollbar;
    }
}