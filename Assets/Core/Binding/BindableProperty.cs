﻿using System;
using UnityEngine.Events;

namespace BridgeUI.Binding
{
    public interface IBindableProperty
    {
        object ValueBoxed { get; set; }
    }

    public class BindableProperty<T> : IBindableProperty
    {
        public event UnityAction<T> onValueChanged = delegate { };

        private T _value = default(T);

        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (!Equals(_value, value))
                {
                    _value = value;
                    ValueChanged(_value);
                }
            }
        }

        public object ValueBoxed
        {
            get { return Value; }
            set { Value = (T)value; }
        }
        public BindableProperty()
        {
        }
        public BindableProperty(T value)
        {
            Value = value;
        }

        private void ValueChanged(T value)
        {
            if (onValueChanged != null)
                onValueChanged.Invoke(value);
        }
        public void RegistValueChanged(UnityAction<T> OnValueChanged)
        {
            this.onValueChanged += OnValueChanged;
        }

        public void RemoveValueChanged(UnityAction<T> OnValueChanged)
        {
            this.onValueChanged -= OnValueChanged;
        }

        public override string ToString()
        {
            return (Value != null ? Value.ToString() : "null");
        }
        public void SetValueNoTrigger(T value)
        {
            _value = value;
        }

        public void Trigger()
        {
            ValueChanged(Value);
        }
        public void Clear()
        {
            _value = default(T);
        }
    }
    public class B_String : BindableProperty<string> { }
    public class B_StringArray : BindableProperty<string[]> { }
    public class B_Int : BindableProperty<int> { }
    public class B_Float : BindableProperty<float> { }
    public class B_Bool : BindableProperty<bool>
    {
      
    }
    public class B_Byte : BindableProperty<byte> { }
    public class B_Long : BindableProperty<long> { }
    public class B_Short : BindableProperty<short> { }
    public class B_Sprite : BindableProperty<UnityEngine.Sprite> { }
    public class B_Texture : BindableProperty<UnityEngine.Texture> { }
    public class B_Action : BindableProperty<System.Action> { }
    public class B_Action<T> : BindableProperty<System.Action<T>> { }
    public class B_UnityAction : BindableProperty<UnityEngine.Events.UnityAction> { }
    public class B_UnityAction<T> : BindableProperty<UnityEngine.Events.UnityAction> { }
    public class B_Func<T> : BindableProperty<System.Func<T>> { }
    public class C_PanelAction : BindableProperty<PanelAction>
    {
        public C_PanelAction() { }
        public C_PanelAction(PanelAction panelAction)
        {
            RegistAction(panelAction);
        }
        public void RegistAction(PanelAction panelAction)
        {
            Value = panelAction;
        }
    }
    public class C_PanelAction<T> : BindableProperty<PanelAction<T>>
    {
        public C_PanelAction() { }
        public C_PanelAction(PanelAction<T> panelAction)
        {
            RegistAction(panelAction);
        }
        public void RegistAction(PanelAction<T> panelAction)
        {
            Value = panelAction;
        }
    }
    public class C_Button : C_PanelAction<UnityEngine.UI.Button> {
        public C_Button() { }
        public C_Button(PanelAction<UnityEngine.UI.Button> panelAction) : base(panelAction) { }
    }
    public class C_Toggle : C_PanelAction<UnityEngine.UI.Toggle> { }
    public class C_Slider : C_PanelAction<UnityEngine.UI.Slider> {
        public C_Slider() { }
        public C_Slider(PanelAction<UnityEngine.UI.Slider> panelAction) : base(panelAction) { }
    }
    public class C_Scrollbar : C_PanelAction<UnityEngine.UI.Scrollbar> { }
    public class C_Dropdown : C_PanelAction<UnityEngine.UI.Dropdown> { }
    public class C_ScrollRect : C_PanelAction<UnityEngine.UI.ScrollRect> { }
    public class C_InputField : C_PanelAction<UnityEngine.UI.InputField> { }
}
