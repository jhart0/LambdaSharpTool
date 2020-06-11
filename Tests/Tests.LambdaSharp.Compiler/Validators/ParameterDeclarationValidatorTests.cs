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

using LambdaSharp.Compiler.Validators;
using Xunit;
using Xunit.Abstractions;

namespace Tests.LambdaSharp.Compiler.Validators {

    public class ParameterDeclarationValidatorTests : _Validator {

        //--- Constructors ---
        public ParameterDeclarationValidatorTests(ITestOutputHelper output) : base(output) { }

        //--- Methods ---

        [Fact]
        public void OnlyNumbersCanHaveMinValue() {

            // arrange
            var module = LoadTestModule();

            // act
            new ParameterDeclarationValidator(this).Validate(module);

            // assert
            ExpectedMessages("ERROR: 'MinValue' attribute can only be used with 'Number' type in Module.yml: line 6, column 15");
        }

        [Fact]
        public void OnlyNumbersCanHaveMaxValue() {

            // arrange
            var module = LoadTestModule();

            // act
            new ParameterDeclarationValidator(this).Validate(module);

            // assert
            ExpectedMessages("ERROR: 'MaxValue' attribute can only be used with 'Number' type in Module.yml: line 6, column 15");
        }

        [Fact]
        public void MinMaxValueMustHaveValidRange() {

            // arrange
            var module = LoadTestModule();

            // act
            new ParameterDeclarationValidator(this).Validate(module);

            // assert
            ExpectedMessages("ERROR: 'MinValue' must be less or equal to 'MaxValue' in Module.yml: line 6, column 15");
        }

        [Fact]
        public void MissingTypeAttribute() {

            // arrange
            var module = LoadTestModule();

            // act
            new ParameterDeclarationValidator(this).Validate(module);

            // assert
            ExpectedMessages("WARNING: missing 'Type' attribute, assuming type 'String' in Module.yml: line 4, column 5");
        }

        [Fact]
        public void MinMaxLengthMustHaveValidRange() {

            // arrange
            var module = LoadTestModule();

            // act
            new ParameterDeclarationValidator(this).Validate(module);

            // assert
            ExpectedMessages(
                "ERROR: 'MinLength' attribute can only be used with 'String' type in Module.yml: line 6, column 16",
                "ERROR: 'MaxLength' attribute can only be used with 'String' type in Module.yml: line 7, column 16"
            );
        }

        [Fact]
        public void AllowedPatternMustBeValidRegex() {

            // arrange
            var module = LoadTestModule();

            // act
            new ParameterDeclarationValidator(this).Validate(module);

            // assert
            ExpectedMessages("ERROR: 'AllowedPattern' attribute must be a valid regular expression in Module.yml: line 6, column 21");
        }
    }
}