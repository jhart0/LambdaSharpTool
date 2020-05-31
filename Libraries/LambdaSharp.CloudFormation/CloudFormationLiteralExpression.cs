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
using System.Text.Json.Serialization;
using LambdaSharp.CloudFormation.Serialization;

namespace LambdaSharp.CloudFormation {

    [JsonConverter(typeof(CloudFormationLiteralExpressionConverter))]
    public class CloudFormationLiteralExpression : ACloudFormationExpression {

        // TODO: support different literal types (e.g. string, number)

        //--- Constructors ---
        public CloudFormationLiteralExpression(string value) => Value = value ?? throw new ArgumentNullException(nameof(value));
        public CloudFormationLiteralExpression(int value) => Value = value.ToString();

        //--- Properties ---
        public string Value { get; }
    }
}