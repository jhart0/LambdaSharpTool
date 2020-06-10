/*
 * LambdaSharp (λ#)
 * Copyright (C) 2018-2020
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

    public sealed class ImportValueFunctionExpression : AFunctionExpression {

        // !ImportValue VALUE
        // NOTE: You can use the following functions in the Fn::ImportValue function. The value of these functions can't depend on a resource.
        //  - Fn::Base64
        //  - Fn::FindInMap
        //  - Fn::If
        //  - Fn::Join
        //  - Fn::Select
        //  - Fn::Split
        //  - Fn::Sub
        //  - Ref

        //--- Fields ---
        private AExpression? _sharedValueToImport;

        //--- Properties ---
        [SyntaxHidden]
        public AExpression SharedValueToImport {
            get => _sharedValueToImport ?? throw new InvalidOperationException();
            set => _sharedValueToImport = SetParent(value ?? throw new ArgumentNullException());
        }

        //--- Methods ---
        public override ASyntaxNode CloneNode() => new ImportValueFunctionExpression {
            SharedValueToImport = SharedValueToImport.Clone()
        };
    }
}