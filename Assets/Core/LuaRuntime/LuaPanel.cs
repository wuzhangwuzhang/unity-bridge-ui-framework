﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Sprites;
using UnityEngine.Scripting;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Assertions.Must;
using UnityEngine.Assertions.Comparers;
using System.Collections;
using System.Collections.Generic;
using XLua;
using System;
using BridgeUI.Binding;

namespace BridgeUI
{
    [LuaCallCSharp][PanelParent]
    public class LuaPanel : PanelBase
    {
        protected class LuaViewModel : Binding.ViewModelBase
        {
            protected LuaTable scriptEnv;

            public LuaViewModel(LuaTable scriptEnv)
            {
                this.scriptEnv = scriptEnv;
            }

            public override BindableProperty<T> GetBindableProperty<T>(string name)
            {
                var prop = base.GetBindableProperty<T>(name);
                if (prop.ValueBoxed == null)
                {
                    prop.Value = scriptEnv.Get<T>(name);
                }
                return prop;
            }
        }

        public TextAsset luaScript;
        internal static LuaEnv luaEnv = new LuaEnv(); //all lua behaviour shared one luaenv only!
        internal static float lastGCTime = 0;
        internal const float GCInterval = 1;//1 second 
        protected LuaTable scriptEnv;
        protected UnityEvent luaAwake = new UnityEvent();
        protected UnityEvent luaOnEnable = new UnityEvent();
        protected UnityEvent luaStart = new UnityEvent();
        protected UnityEvent luaUpdate = new UnityEvent();
        protected UnityEvent luaOnDisable = new UnityEvent();
        protected UnityEvent luaOnDestroy = new UnityEvent();

        protected LuaViewModel luaViewModel { get { return BindingContext as LuaViewModel; } }

        protected override void Awake()
        {
            base.Awake();
            InitScritEnv();
            RegistBaseAction();
            BindingContext = new LuaViewModel(scriptEnv);
            luaAwake.Invoke();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            luaOnEnable.Invoke();
        }
        protected override void Start()
        {
            base.Start();
            luaStart.Invoke();
        }

        protected virtual void Update()
        {
            luaUpdate.Invoke();
            if (Time.time - lastGCTime > GCInterval)
            {
                luaEnv.Tick();
                lastGCTime = Time.time;
            }
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            luaOnDisable.Invoke();
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            luaOnDestroy.Invoke();
            scriptEnv.Dispose();
        }

        private void InitScritEnv()
        {
            scriptEnv = luaEnv.NewTable();

            LuaTable meta = luaEnv.NewTable();
            meta.Set("__index", luaEnv.Global);
            scriptEnv.SetMetaTable(meta);
            meta.Dispose();

            scriptEnv.Set("self", this);
            luaEnv.DoString(luaScript.text, "LuaPanel", scriptEnv);
        }

        private void RegistBaseAction()
        {
            Binder.RegistNormalEvent(luaAwake, "awake");
            Binder.RegistNormalEvent(luaOnEnable, "onenable");
            Binder.RegistNormalEvent(luaStart, "start");
            Binder.RegistNormalEvent(luaUpdate,"update");
            Binder.RegistNormalEvent(luaOnDisable, "ondisable");
            Binder.RegistNormalEvent(luaOnDestroy, "ondestroy");
        }

        protected override void HandleData(object data)
        {
            base.HandleData(data);

            if (luaViewModel != null)
            {
               var action =  luaViewModel.GetBindableProperty<UnityAction<object>>("handle_data");
                if(action.Value != null)
                {
                    action.Value.Invoke(data);
                }
            }
        }

    }

}