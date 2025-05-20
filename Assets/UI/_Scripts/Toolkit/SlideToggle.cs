using UnityEngine;
using UnityEngine.UIElements;

namespace LM
{
    [UxmlElement]
    public partial class SlideToggle : BaseField<bool>
    {
        public static readonly new string ussClassName = "slide-toggle";
        public static readonly new string inputUssClassName = "slide-toggle__input";
        public static readonly string inputKnobUssClassName = "slide-toggle__input-knob";
        public static readonly string inputCheckedUssClassName = "slide-toggle__input--checked";
        public static readonly string stateLabelUssClassName = "slide-toggle__state-label";

        private bool m_IsOn;
        
        public string OnLabel { get; set; } = "On"; // Default value
        public string OffLabel { get; set; } = "Off"; // Default value
        
        [UxmlAttribute("IsOn")]
        public bool IsOn
        {
            get => m_IsOn;
            set
            {
                if (m_IsOn == value) return;

                m_IsOn = value;
                UpdateStateLabel();
                SetValueWithoutNotify(m_IsOn);
            }
        }

        private VisualElement m_Input; // Input part of the toggle.
        private VisualElement m_Knob; // Knob element.
        private Label m_StateLabel; // On/off label.
        
        public SlideToggle() : this(null) { }
        
        public SlideToggle(string label) : base(label, null)
        {
            AddToClassList(ussClassName);
            
            m_Input = this.Q(className: BaseField<bool>.inputUssClassName);
            m_Input.AddToClassList(inputUssClassName);
            Add(m_Input);
            
            m_Knob = new();
            m_Knob.AddToClassList(inputKnobUssClassName);
            m_Input.Add(m_Knob);

            m_StateLabel = new Label();
            m_StateLabel.AddToClassList(stateLabelUssClassName);
            m_Input.Add(m_StateLabel);
            
            RegisterCallback<ClickEvent>(evt => OnClick(evt));
            RegisterCallback<KeyDownEvent>(evt => OnKeydownEvent(evt));
            RegisterCallback<NavigationSubmitEvent>(evt => OnSubmit(evt));
            UpdateStateLabel();
        }
        
        private static void OnClick(ClickEvent evt)
        {
            var slideToggle = evt.currentTarget as SlideToggle;
            slideToggle.ToggleValue();

            evt.StopPropagation();
        }
        
        private static void OnSubmit(NavigationSubmitEvent evt)
        {
            var slideToggle = evt.currentTarget as SlideToggle;

            if (slideToggle == null)
                return;

            slideToggle.ToggleValue();

            evt.StopPropagation();
        }
        
        private static void OnKeydownEvent(KeyDownEvent evt)
        {
            var slideToggle = evt.currentTarget as SlideToggle;
            if (slideToggle == null) return;
            if (slideToggle.panel.contextType == ContextType.Player) return;
            
            if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
            {
                slideToggle.ToggleValue();
                evt.StopPropagation();
            }
        }
        
        private void ToggleValue()
        {
            value = !value;
            UpdateStateLabel();
        }
        
        public override void SetValueWithoutNotify(bool newValue)
        {
            base.SetValueWithoutNotify(newValue);
            
            m_Input.EnableInClassList(inputCheckedUssClassName, newValue);
            UpdateStateLabel();
        }
        
        private void UpdateStateLabel()
        {
            if (value) // If the toggle is on
            {
                m_StateLabel.text = OnLabel;
            }
            else
            {
                m_StateLabel.text = OffLabel;
            }
        }
    }
}
