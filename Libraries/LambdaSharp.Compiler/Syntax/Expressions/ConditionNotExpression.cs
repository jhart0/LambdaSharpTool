﻿/*
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

namespace LambdaSharp.Compiler.Syntax.Expressions {

    public sealed class ConditionNotExpression : AConditionExpression {

        // parse !Not [ EXPR ]
        // NOTE: You can use the following functions in a Fn::Not function:
        //  - Fn::FindInMap
        //  - Ref
        //  - Condition
        //  - Other condition functions

        //--- Fields ---
        private AExpression? _value;

        //--- Properties ---
        public AExpression Value {
            get => _value ?? throw new InvalidOperationException();
            set => _value = SetParent(value ?? throw new ArgumentNullException());
        }

        //--- Methods ---
        public override ASyntaxNode CloneNode() => new ConditionNotExpression {
            Value = Value.Clone()
        };
    }
}