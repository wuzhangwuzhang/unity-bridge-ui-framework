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
using System.CodeDom;
using BridgeUI.Model;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using System;

namespace BridgeUI.CodeGen
{
    public class ComponentCoder
    {
        protected  MethodDeclaration InitComponentsNode;
        protected  MethodDeclaration PropBindingsNode;
        protected  TypeDeclaration classNode;

        public void SetContext(TypeDeclaration classNode)
        {
            this.classNode = classNode;
            InitComponentsNode = classNode.Descendants.OfType<MethodDeclaration>().Where(x => x.Name == GenCodeUtil.initcomponentMethod).FirstOrDefault();
            PropBindingsNode = classNode.Descendants.OfType<MethodDeclaration>().Where(x => x.Name == GenCodeUtil.propbindingsMethod).FirstOrDefault();
        }

        /// <summary>
        /// Binding关联
        /// </summary>
        /// <returns></returns>
        public virtual void CompleteCode(ComponentItem component)
        {
            foreach (var item in component.viewItems)
            {
                BindingInvocations(component.name, item);
            }

            foreach (var item in component.eventItems)
            {
                if (item.runtime)
                {
                    BindingInvocations(component.name,item);
                }
                else
                {
                    LocalInvocations(component.name,item);
                }
            }

            //if (!component.binding)
            //{
            //    var invocations = InitComponentsNode.Body.Descendants.OfType<InvocationExpression>();
            //    var invocation = invocations.Where(x => x.Target.ToString().Contains("m_" + component.name) && x.Arguments.Where(ag => ag.ToString() == component.sourceName) != null).FirstOrDefault();
            //    var methodName = GetMethodName_InitComponentsNode(component.componentType);
            //    if (invocation == null && !string.IsNullOrEmpty(methodName) && !string.IsNullOrEmpty(component.sourceName))
            //    {
            //        invocation = new InvocationExpression();
            //        invocation.Target = new MemberReferenceExpression(new MemberReferenceExpression(new IdentifierExpression("m_" + component.name), methodName, new AstType[0]), "AddListener", new AstType[0]);
            //        invocation.Arguments.Add(new IdentifierExpression(component.sourceName));
            //        InitComponentsNode.Body.Add(invocation);
            //    }
            //}
            //else
            //{
            //    var invocations = PropBindingsNode.Body.Descendants.OfType<InvocationExpression>();
            //    var invocation = invocations.Where(x => x.Target.ToString().Contains("Binder") && x.Arguments.Count > 0 && x.Arguments.First().ToString().Contains("m_" + component.name)).FirstOrDefault();
            //    if (invocation == null)
            //    {
            //        var methodName = GetMethodNameFromComponent(component.componentType);
            //        if (!string.IsNullOrEmpty(methodName))
            //        {
            //            invocation = new InvocationExpression();
            //            invocation.Target = new MemberReferenceExpression(new IdentifierExpression("Binder"), methodName, new AstType[0]);
            //            invocation.Arguments.Add(new IdentifierExpression("m_" + component.name));
            //            invocation.Arguments.Add(new PrimitiveExpression(component.sourceName));
            //            PropBindingsNode.Body.Add(invocation);
            //        }

            //    }
            //}
        }

        /// <summary>
        /// 远端关联
        /// </summary>
        protected virtual void BindingInvocations(string name,BindingShow bindingInfo)
        {
            var invocations = PropBindingsNode.Body.Descendants.OfType<InvocationExpression>();
            var arg0_name = "m_" + name + "." + bindingInfo.bindingTarget;
            var invocation = invocations.Where(x => x.Target.ToString().Contains("Binder") && x.Arguments.Count > 0 && x.Arguments.First().ToString().Contains(arg0_name)).FirstOrDefault();
            if (invocation == null)
            {
                var typeName = bindingInfo.bindingTargetType.typeName;
                var methodName = string.Format( "RegistMember<{0}>",typeName);
                if (!string.IsNullOrEmpty(methodName))
                {
                    invocation = new InvocationExpression();
                    invocation.Target = new MemberReferenceExpression(new IdentifierExpression("Binder"), methodName, new AstType[0]);
                    invocation.Arguments.Add(new PrimitiveExpression(arg0_name));
                    invocation.Arguments.Add(new PrimitiveExpression(bindingInfo.bindingSource));
                    PropBindingsNode.Body.Add(invocation);
                }

            }
        }
        /// <summary>
        /// 远端关联
        /// </summary>
        /// <param name="type"></param>
        /// <param name="bindingInfo"></param>
        protected virtual void BindingInvocations(string name, BindingEvent bindingInfo)
        {
            var invocations = PropBindingsNode.Body.Descendants.OfType<InvocationExpression>();
            var arg0_name = "m_" + name + "." + bindingInfo.bindingTarget;
            var invocation = invocations.Where(x => x.Target.ToString().Contains("Binder") && x.Arguments.Count > 0 && x.Arguments.First().ToString().Contains(arg0_name)).FirstOrDefault();
            if (invocation == null)
            {
                var methodName = "RegistEvent";
                if (!string.IsNullOrEmpty(methodName))
                {
                    invocation = new InvocationExpression();
                    invocation.Target = new MemberReferenceExpression(new IdentifierExpression("Binder"), methodName, new AstType[0]);
                    invocation.Arguments.Add(new IdentifierExpression(arg0_name));
                    invocation.Arguments.Add(new PrimitiveExpression(bindingInfo.bindingSource));
                    PropBindingsNode.Body.Add(invocation);
                }

            }
        }

        /// <summary>
        /// 本地关联
        /// </summary>
        protected virtual void LocalInvocations(string name, BindingEvent bindingInfo)
        {
            var invocations = InitComponentsNode.Body.Descendants.OfType<InvocationExpression>();
            var invocation = invocations.Where(x => x.Target.ToString().Contains("m_" + name) && x.Arguments.Where(ag => ag.ToString() == bindingInfo.bindingSource) != null).FirstOrDefault();
            var eventName = bindingInfo.bindingTarget;//如onClick
            if (invocation == null && !string.IsNullOrEmpty(eventName) && !string.IsNullOrEmpty(bindingInfo.bindingSource))
            {
                invocation = new InvocationExpression();
                invocation.Target = new MemberReferenceExpression(new MemberReferenceExpression(new IdentifierExpression("m_" + name), eventName, new AstType[0]), "AddListener", new AstType[0]);
                invocation.Arguments.Add(new IdentifierExpression(bindingInfo.bindingSource));
                InitComponentsNode.Body.Add(invocation);
                CompleteMethod(bindingInfo);
            }
        }

        /// <summary>
        /// 完善本地绑定的方法
        /// </summary>
        /// <param name="item"></param>
        protected void CompleteMethod(BindingEvent item)
        {
            var funcNode = classNode.Descendants.OfType<MethodDeclaration>().Where(x => x.Name == item.bindingSource).FirstOrDefault();
            if (funcNode == null)
            {
                var parameter = item.bindingTargetType.type.GetMethod("AddListener").GetParameters()[0];
                List<ParameterDeclaration> arguments = new List<ParameterDeclaration>();
                var parameters = parameter.ParameterType.GetGenericArguments();
                int count = 0;
                foreach (var para in parameters)
                {
                    ParameterDeclaration argument = new ParameterDeclaration(new ICSharpCode.NRefactory.CSharp.PrimitiveType(para.Name), "arg" + count++);
                    arguments.Add(argument);
                }

                {
                    funcNode = new MethodDeclaration();
                    funcNode.Name = item.bindingSource;
                    funcNode.Modifiers |= Modifiers.Protected;
                    funcNode.ReturnType = new ICSharpCode.NRefactory.CSharp.PrimitiveType("void");
                    funcNode.Parameters.AddRange(arguments);
                    funcNode.Body = new BlockStatement();
                    classNode.AddChild(funcNode, Roles.TypeMemberRole);
                }

            }
        }
    }
}