using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PerkButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image buttonIcon;

    [Header("Highlight")]
    public Image background;

    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.8f, 1f, 0.8f);
    public Color selectedColor = new Color(1f, 0.9f, 0.4f);

    [Header("Hover Juice")]
    public float hoverScaleMultiplier = 1.08f;
    public float minHoverRotation = 2f;
    public float maxHoverRotation = 5f;
    public float animSpeed = 10f;

    [HideInInspector] public AbilityUpgradeSO perkAsset;

    private PerkManager _manager;
    private Ability _targetAbility;

    private bool isSelected;
    private bool isHovered;

    // Cached initial transform
    private Vector3 _initialBgScale;
    private Quaternion _initialBgRotation;

    // Animation targets
    private Vector3 _targetBgScale;
    private Quaternion _targetBgRotation;

    public void SetupButton(PerkManager manager, string abilityName, AbilityUpgradeSO asset)
    {
        _manager = manager;
        perkAsset = asset;

        if (manager.abilityDatabase != null)
        {
            _targetAbility = manager.abilityDatabase.GetAbilityByName(abilityName);

            if (_targetAbility != null && buttonIcon != null)
                buttonIcon.sprite = _targetAbility.icon;
        }

        if (background != null)
        {
            // Cache whatever scale/rotation you set in inspector
            _initialBgScale = background.rectTransform.localScale;
            _initialBgRotation = background.rectTransform.localRotation;

            _targetBgScale = _initialBgScale;
            _targetBgRotation = _initialBgRotation;
        }

        SetNormal();
    }

    private void Update()
    {
        if (background == null) return;

        RectTransform bgRect = background.rectTransform;

        bgRect.localScale = Vector3.Lerp(
            bgRect.localScale,
            _targetBgScale,
            Time.deltaTime * animSpeed
        );

        bgRect.localRotation = Quaternion.Lerp(
            bgRect.localRotation,
            _targetBgRotation,
            Time.deltaTime * animSpeed
        );
    }

    public void OnSelect()
    {
        Debug.Log($"[PerkButton] Selected → {_targetAbility?.abilityName}");

        if (_manager != null)
        {
            _manager.SelectPerk(perkAsset, _targetAbility);
            _manager.NotifySelection(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;

        if (!isSelected)
            SetHover();

        // Random left/right (50/50)
        float direction = Random.value < 0.5f ? -1f : 1f;

        // Random rotation amount
        float angle = Random.Range(minHoverRotation, maxHoverRotation) * direction;

        // Scale relative to initial inspector scale
        _targetBgScale = _initialBgScale * hoverScaleMultiplier;

        // Rotate relative to initial inspector rotation
        _targetBgRotation = _initialBgRotation * Quaternion.Euler(0f, 0f, angle);

        if (_manager != null && perkAsset != null && _targetAbility != null)
        {
            _manager.DisplayPerkDetails(perkAsset, _targetAbility);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;

        if (!isSelected)
            SetNormal();

        // Restore original transform
        _targetBgScale = _initialBgScale;
        _targetBgRotation = _initialBgRotation;

        if (_manager != null)
        {
            _manager.RestoreSelectedPerk();
        }
    }

    public void SetSelected(bool value)
    {
        isSelected = value;

        if (isSelected)
        {
            SetSelectedColor();

            // subtle selected scale relative to initial
            _targetBgScale = _initialBgScale * 1.03f;
            _targetBgRotation = _initialBgRotation;
        }
        else
        {
            SetNormal();

            if (!isHovered)
            {
                _targetBgScale = _initialBgScale;
                _targetBgRotation = _initialBgRotation;
            }
        }
    }

    private void SetNormal()
    {
        if (background != null)
            background.color = normalColor;
    }

    private void SetHover()
    {
        if (background != null)
            background.color = hoverColor;
    }

    private void SetSelectedColor()
    {
        if (background != null)
            background.color = selectedColor;
    }
}