﻿#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq.Mapping;
using System.Reflection;
using DbLinq.Util;

#if MONO_STRICT
namespace System.Data.Linq.Implementation
#else
namespace DbLinq.Data.Linq.Implementation
#endif
{
    /// <summary>
    /// ModificationHandler class handles entities in two ways:
    /// 1. if entity implements IModifed, uses the interface and its IsModifed flag property
    /// 2. otherwise, the handler keeps a dictionary of raw data per entity
    /// </summary>
    internal class MemberModificationHandler : IMemberModificationHandler
    {
        private readonly IDictionary<object, IDictionary<string, object>> rawDataEntities = new Dictionary<object, IDictionary<string, object>>(new ReferenceEqualityComparer<object>());
        private readonly IDictionary<object, IDictionary<string, MemberInfo>> modifiedProperties = new Dictionary<object, IDictionary<string, MemberInfo>>(new ReferenceEqualityComparer<object>());

        protected virtual IEnumerable<MemberInfo> GetColumnMembers(Type entityType, MetaModel metaModel)
        {
            foreach (var dataMember in metaModel.GetTable(entityType).RowType.PersistentDataMembers)
            {
                yield return dataMember.Member;
            }
        }

        protected bool IsPrimitiveType(Type t)
        {
            if (t.IsValueType)
                return true;
            if (t == typeof(string))
                return true;
            return false;
        }

        /// <summary>
        /// Adds simple (value) properties of an object to a given dictionary
        /// and recurses if a property contains complex data
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="rawData"></param>
        /// <param name="prefix"></param>
        /// <param name="metaModel"></param>
        protected void AddRawData(object entity, IDictionary<string, object> rawData, string prefix, MetaModel metaModel)
        {
            if (entity == null)
                return;
            foreach (var memberInfo in GetColumnMembers(entity.GetType(), metaModel))
            {
                object propertyValue = memberInfo.GetMemberValue(entity);
                // if it is a value, it can be stored directly
                if (IsPrimitiveType(memberInfo.GetMemberType()))
                {
                    rawData[prefix + memberInfo.Name] = propertyValue;
                }
                else // otherwise, we recurse, and prefix the current property name to sub properties to avoid conflicts
                {
                    AddRawData(propertyValue, rawData, memberInfo.Name + ".", metaModel);
                }
            }
        }

        /// <summary>
        /// Creates a "flat view" from a composite object
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="metaModel"></param>
        /// <returns>a pair of {property name, property value}</returns>
        protected IDictionary<string, object> GetEntityRawData(object entity, MetaModel metaModel)
        {
            var rawData = new Dictionary<string, object>();
            AddRawData(entity, rawData, string.Empty, metaModel);
            return rawData;
        }

        /// <summary>
        /// Tells if the object notifies a change
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private bool IsNotifying(object entity)
        {
            return entity is INotifyPropertyChanged
                   || entity is INotifyPropertyChanging;
        }

        /// <summary>
        /// Start to watch an entity. From here, changes will make IsModified() return true
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="metaModel"></param>
        public void Register(object entity, MetaModel metaModel)
        {
            Register(entity, entity, metaModel);
        }

        /// <summary>
        /// Start to watch an entity. From here, changes will make IsModified() return true if the entity has changed
        /// If the entity is already registered, there's no error, but the entity is reset to its original state
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entityOriginalState"></param>
        /// <param name="metaModel"></param>
        public void Register(object entity, object entityOriginalState, MetaModel metaModel)
        {
            // notifying, we need to wait for changes
            if (IsNotifying(entity))
            {
                RegisterNotification(entity, entityOriginalState, metaModel);
            }
            // raw data, we keep a snapshot of the current state
            else
            {
                lock (rawDataEntities)
                {
                    if (!rawDataEntities.ContainsKey(entity) && entityOriginalState != null)
                        rawDataEntities[entity] = GetEntityRawData(entityOriginalState, metaModel);
                }
            }
        }

        private void RegisterNotification(object entity, object entityOriginalState, MetaModel metaModel)
        {
            lock (modifiedProperties)
            {
                if (modifiedProperties.ContainsKey(entity))
                    return;
                modifiedProperties[entity] = new Dictionary<string, MemberInfo>();
                if (entity is INotifyPropertyChanging)
                {
                    ((INotifyPropertyChanging)entity).PropertyChanging += (OnPropertyChangingEvent);
                }

                if (entity is INotifyPropertyChanged)
                {
                    ((INotifyPropertyChanged)entity).PropertyChanged += (OnPropertyChangedEvent);
                }
                // then check all properties, and note them as changed if they already did
                if (!ReferenceEquals(entity, entityOriginalState)) // only if we specified another original entity
                {
                    foreach (var dataMember in metaModel.GetTable(entity.GetType()).RowType.PersistentDataMembers)
                    {
                        var memberInfo = dataMember.Member;
                        if (entityOriginalState == null ||
                            IsPropertyModified(memberInfo.GetMemberValue(entity),
                                               memberInfo.GetMemberValue(entityOriginalState)))
                        {
                            SetPropertyChanged(entity, memberInfo.Name);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Occurs on INotifyPropertyChanging.PropertyChanging
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPropertyChangingEvent(object sender, PropertyChangingEventArgs e)
        {
            //SetPropertyChanged(sender, e.PropertyName);
        }

        /// <summary>
        /// Occurs on INotifyPropertyChanged.PropertyChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPropertyChangedEvent(object sender, PropertyChangedEventArgs e)
        {
            SetPropertyChanged(sender, e.PropertyName);
        }

        /// <summary>
        /// Unregisters an entity.
        /// This is useful when it is switched from update to delete list
        /// </summary>
        /// <param name="entity"></param>
        public void Unregister(object entity)
        {
            if (IsNotifying(entity))
                UnregisterNotification(entity);
            else
            {
                lock (rawDataEntities)
                {
                    if (rawDataEntities.ContainsKey(entity))
                        rawDataEntities.Remove(entity);
                }
            }
        }

        private void UnregisterNotification(object entity)
        {
            lock (modifiedProperties)
            {
                if (!modifiedProperties.ContainsKey(entity))
                    return;
                modifiedProperties.Remove(entity);
            }
            if (entity is INotifyPropertyChanging)
            {
                ((INotifyPropertyChanging)entity).PropertyChanging -= OnPropertyChangingEvent;
            }
            else if (entity is INotifyPropertyChanged)
            {
                ((INotifyPropertyChanged)entity).PropertyChanged -= OnPropertyChangedEvent;
            }
        }

        /// <summary>
        /// This method is called when a notifying object sends an event because of a property change
        /// We may keep track of the precise change in the future
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        private void SetPropertyChanged(object entity, string propertyName)
        {
            lock (modifiedProperties)
            {
                PropertyInfo pi=GetProperty(entity, propertyName);
                if(pi==null)
                    throw new ArgumentException("Incorrect property changed");

                modifiedProperties[entity][propertyName] = pi;
            }
        }

        /// <summary>
        /// Returns if the entity was modified since it has been Register()ed for the first time
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="metaModel"></param>
        /// <returns></returns>
        public bool IsModified(object entity, MetaModel metaModel)
        {
            // 1. event notifying case (INotify*)
            if (IsNotifying(entity))
                return IsNotifyingModified(entity);

            // 2. raw data
            return IsRawModified(entity, metaModel);
        }

        private bool IsNotifyingModified(object entity)
        {
            lock (modifiedProperties)
            {
                return !modifiedProperties.ContainsKey(entity) || modifiedProperties[entity].Count > 0;
            }
        }

        private bool IsPropertyModified(object p1, object p2)
        {
            return !object.Equals(p1, p2);
        }

        private bool IsRawModified(object entity, MetaModel metaModel)
        {
            lock (rawDataEntities)
            {
                // if not present, maybe it was inserted (or set to dirty)
                // TODO: this will be useless when we will support the differential properties
                if (!rawDataEntities.ContainsKey(entity))
                    return true;

                IDictionary<string, object> originalData = rawDataEntities[entity];
                IDictionary<string, object> currentData = GetEntityRawData(entity, metaModel);

                foreach (string key in originalData.Keys)
                {
                    object originalValue = originalData[key];
                    object currentValue = currentData[key];
                    if (IsPropertyModified(originalValue, currentValue))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns a list of all modified properties since last Register/ClearModified
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="metaModel"></param>
        /// <returns></returns>
        public IList<MemberInfo> GetModifiedProperties(object entity, MetaModel metaModel)
        {
            if (IsNotifying(entity))
                return GetNotifyingModifiedProperties(entity, metaModel);

            return GetRawModifiedProperties(entity, metaModel);
        }

        protected IList<MemberInfo> GetAllColumnProperties(object entity, MetaModel metaModel)
        {
            if (entity == null)
                throw new ArgumentNullException("GetAllColumnProperties(): entity must not be null");
            var properties = new List<MemberInfo>(GetColumnMembers(entity.GetType(), metaModel));
            return properties;
        }

        protected IList<MemberInfo> GetSelfDeclaringModifiedProperties(object entity, MetaModel metaModel)
        {
            return GetAllColumnProperties(entity, metaModel);
        }

        protected IList<MemberInfo> GetNotifyingModifiedProperties(object entity, MetaModel metaModel)
        {
            lock (modifiedProperties)
            {
                IDictionary<string, MemberInfo> properties;
                // if we don't have it, it is fully dirty
                if (!modifiedProperties.TryGetValue(entity, out properties))
                    return GetAllColumnProperties(entity, metaModel);
                return new List<MemberInfo>(properties.Values);
            }
        }

        protected IList<MemberInfo> GetRawModifiedProperties(object entity, MetaModel metaModel)
        {
            var properties = new List<MemberInfo>();

            IDictionary<string, object> originalData;
            lock (rawDataEntities)
            {
                if (!rawDataEntities.TryGetValue(entity, out originalData))
                    return GetAllColumnProperties(entity, metaModel);
            }
            IDictionary<string, object> currentData = GetEntityRawData(entity, metaModel);

            foreach (string key in currentData.Keys)
            {
                object currentValue = currentData[key];
                object originalValue = originalData[key];
                if (IsPropertyModified(originalValue, currentValue))
                    properties.Add(GetProperty(entity, key));
            }

            return properties;
        }

        public void ClearModified(object entity, MetaModel metaModel)
        {
            if (IsNotifying(entity))
                ClearNotifyingModified(entity);
            else
                ClearRawModified(entity, metaModel);
        }

        private void ClearNotifyingModified(object entity)
        {
            lock (modifiedProperties)
            {
                modifiedProperties[entity] = new Dictionary<string, MemberInfo>();
            }
        }

        private void ClearRawModified(object entity, MetaModel metaModel)
        {
            lock (rawDataEntities)
            {
                rawDataEntities[entity] = GetEntityRawData(entity, metaModel);
            }
        }

        private PropertyInfo GetProperty(object entity, string propertyName)
        {
            return entity.GetType().GetProperty(propertyName);
        }
    }
}
