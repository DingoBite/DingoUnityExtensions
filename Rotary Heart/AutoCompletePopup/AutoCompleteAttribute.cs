using System;
using UnityEngine;

namespace RotaryHeart.Lib.AutoComplete
{
    public class AutoCompleteAttribute : PropertyAttribute
    {
        public string MemberName { get; }
        public string[] Entries { get; }
        
        public AutoCompleteAttribute(string[] entries)
        {
            Entries = entries;
        }

        public AutoCompleteAttribute(string memberName)
        {
            MemberName = memberName;
        }
    }
    
    public class AutoCompleteTextFieldAttribute : AutoCompleteAttribute
    {
        public bool AllowCustom { get; }
        public bool AllowEmpty { get; }
        public bool ReturnFullPath { get; }
        public string Separator { get; }
        public string OrderFuncMember { get; }
        
        public AutoCompleteTextFieldAttribute(string[] entries) : base(entries) { }
        public AutoCompleteTextFieldAttribute(string memberName) : base(memberName){}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="memberName"></param>
        /// <param name="allowCustom"></param>
        /// <param name="allowEmpty"></param>
        /// <param name="orderFuncMember">Must be function or member that return Func(string s) -> float</param>
        public AutoCompleteTextFieldAttribute(string memberName, bool allowCustom = false, bool allowEmpty = true, bool returnFullPath = false, string separator = "/", string orderFuncMember = null) : base(memberName)
        {
            AllowCustom = allowCustom;
            AllowEmpty = allowEmpty;
            ReturnFullPath = returnFullPath;
            Separator = separator;
            OrderFuncMember = orderFuncMember;
        }
    }
    
    public class AutoCompleteDropDownAttribute : AutoCompleteAttribute
    {
        public bool AllowCustom { get; }
        public bool AllowEmpty { get; }
        public bool ReturnFullPath { get; }
        public string Separator { get; }
        public string OrderFuncMember { get; }

        public AutoCompleteDropDownAttribute(string[] entries) : base(entries) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memberName"></param>
        /// <param name="allowCustom"></param>
        /// <param name="allowEmpty"></param>
        /// <param name="orderFuncMember">Must be function or member that return Func(string s) -> float</param>
        public AutoCompleteDropDownAttribute(string memberName, bool allowCustom = false, bool allowEmpty = true, bool returnFullPath = false, string separator = "/", string orderFuncMember = null) : base(memberName)
        {
            AllowCustom = allowCustom;
            AllowEmpty = allowEmpty;
            ReturnFullPath = returnFullPath;
            Separator = separator;
            OrderFuncMember = orderFuncMember;
        }
    }
    
    
}