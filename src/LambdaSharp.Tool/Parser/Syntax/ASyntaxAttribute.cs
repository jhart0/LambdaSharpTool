/*
 * LambdaSharp (λ#)
 * Copyright (C) 2018-2019
 * lambdasharp.net
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

namespace LambdaSharp.Tool.Parser.Syntax {

    public enum SyntaxType {
        Required,
        Optional,
        Keyword
    }

    [AttributeUsage(AttributeTargets.Property)]
    public abstract class ASyntaxAttribute : Attribute {

        //--- Constructors ---
        public ASyntaxAttribute(SyntaxType type) => Type = type;

        //--- Properties ---
        public SyntaxType Type { get; private set; } = SyntaxType.Required;
    }

    public class SyntaxRequiredAttribute : ASyntaxAttribute {

        //--- Constructors ---
        public SyntaxRequiredAttribute() : base(SyntaxType.Required) { }
    }

    public class SyntaxOptionalAttribute : ASyntaxAttribute {

        //--- Constructors ---
        public SyntaxOptionalAttribute() : base(SyntaxType.Optional) { }
    }

    public class SyntaxKeywordAttribute : ASyntaxAttribute {

        //--- Constructors ---
        public SyntaxKeywordAttribute() : base(SyntaxType.Keyword) { }
    }
}