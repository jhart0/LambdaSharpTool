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
using LambdaSharp.Compiler.Syntax.Expressions;

namespace LambdaSharp.Compiler.Syntax.EventSources {

    [SyntaxDeclarationKeyword("SlackCommand")]
    public sealed class SlackCommandEventSourceDeclaration : AEventSourceDeclaration {

        //--- Constructors ---
        public SlackCommandEventSourceDeclaration(LiteralExpression eventSource) => EventSource = SetParent(eventSource ?? throw new ArgumentNullException(nameof(eventSource)));

        //--- Properties ---
        public string[]? SlackPath { get; set; }
        public LiteralExpression EventSource { get; }
    }
}