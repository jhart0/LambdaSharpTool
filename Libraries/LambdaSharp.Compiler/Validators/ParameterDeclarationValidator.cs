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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LambdaSharp.Compiler.Syntax;

namespace LambdaSharp.Compiler.Validators {
    using ErrorFunc = Func<string, Error>;

    public sealed class ParameterDeclarationValidator {

        //--- Constants ---
        private const int MAX_PARAMETER_DESCRIPTION_LENGTH = 4_000;

        // TODO: validate max parameter length
        private const int MAX_PARAMETER_VALUE_LENGTH = 4_000;

        //--- Class Fields ---
        #region Errors/Warnings

        // Structure
        private static readonly Error ParameterDeclarationCannotBeNested = new Error(0, "Parameter declaration cannot be nested in a Group");

        // Properties: Type
        private static readonly ErrorFunc UnknownType = parameter => new Error(0, $"unknown paremeter type '{parameter}'");

        // Properties: MinLength, MaxLength
        private static readonly Error MinLengthAttributeRequiresStringType = new Error(0, "'MinLength' attribute can only be used with 'String' type");
        private static readonly Error MinLengthMustBeAnInteger = new Error(0, "'MinLength' must be an integer");
        private static readonly Error MinLengthMustBeNonNegative = new Error(0, $"'MinLength' must be greater or equal than 0");
        private static readonly Error MinLengthTooLarge = new Error(0, $"'MinLength' cannot exceed' {MAX_PARAMETER_VALUE_LENGTH:N0}");
        private static readonly Error MinMaxLengthInvalidRange = new Error(0, "'MinLength' must be less or equal to 'MaxLength'");
        private static readonly Error MaxLengthAttributeRequiresStringType = new Error(0, "'MaxLength' attribute can only be used with 'String' type");
        private static readonly Error MaxLengthMustBeAnInteger = new Error(0, "'MaxLength' must be an integer");
        private static readonly Error MaxLengthMustBePositive = new Error(0, "'MaxLength' must be greater than 0");
        private static readonly Error MaxLengthTooLarge = new Error(0, $"'MaxLength' cannot exceed' {MAX_PARAMETER_VALUE_LENGTH:N0}");

        // Properties: MinValue, MaxValue
        private static readonly Error MinValueAttributeRequiresNumberType = new Error(0, "'MinValue' attribute can only be used with 'Number' type");
        private static readonly Error MinValueMustBeAnInteger = new Error(0, "'MinValue' must be an integer");
        private static readonly Error MaxValueAttributeRequiresNumberType = new Error(0, "'MaxValue' attribute can only be used with 'Number' type");
        private static readonly Error MinMaxValueInvalidRange = new Error(0, "'MinValue' must be less or equal to 'MaxValue'");
        private static readonly Error MaxValueMustBeAnInteger = new Error(0, "'MaxValue' must be an integer");

        // Properties: AllowedPattern, ConstraintDescription
        private static readonly Error AllowedPatternAttributeRequiresStringType = new Error(0, "'AllowedPattern' attribute can only be used with 'String' type");
        private static readonly Error AllowedPatternAttributeInvalid = new Error(0, "'AllowedPattern' attribute must be a regular expression");
        private static readonly Error ConstraintDescriptionAttributeRequiresStringType = new Error(0, "'ConstraintDescription' attribute can only be used with 'String' type");
        private static readonly Error ConstraintDescriptionAttributeRequiresAllowedPatternAttribute = new Error(0, "'ConstraintDescription' attribute requires 'AllowedPattern' attribute to be set");

        // Properties: Description
        private static readonly Error DescriptionAttributeExceedsSizeLimit = new Error(0, $"'Description' attribute cannot exceed {MAX_PARAMETER_DESCRIPTION_LENGTH:N0} characters");

        // Properties: EncryptionContext
        private static readonly Error EncryptionContextAttributeRequiresSecretType = new Error(0, "'EncryptionContext' attribute can only be used with 'Secret' type");
        private static readonly Error EncryptionContextExpectedLiteralStringExpression = new Error(0, "'EncryptionContext' expected literal string expression");

        // Properties: Allow
        public static readonly Error AllowAttributeRequiresCloudFormationType = new Error(0, "'Allow' attribute can only be used with a CloudFormation type");
        #endregion

        private static readonly HashSet<string> _cloudFormationParameterTypes = new HashSet<string> {

            // Literal Types
            "String",
            "Number",
            "List<Number>",
            "CommaDelimitedList",

            // Parameter Store Keys & Values
            "AWS::SSM::Parameter::Name",
            "AWS::SSM::Parameter::Value<String>",
            "AWS::SSM::Parameter::Value<List<String>>",
            "AWS::SSM::Parameter::Value<CommaDelimitedList>",

            // Validated Resource Types
            "AWS::EC2::AvailabilityZone::Name",
            "AWS::EC2::Image::Id",
            "AWS::EC2::Instance::Id",
            "AWS::EC2::KeyPair::KeyName",
            "AWS::EC2::SecurityGroup::GroupName",
            "AWS::EC2::SecurityGroup::Id",
            "AWS::EC2::Subnet::Id",
            "AWS::EC2::Volume::Id",
            "AWS::EC2::VPC::Id",
            "AWS::Route53::HostedZone::Id",

            // List of Validated Resource Types
            "List<AWS::EC2::AvailabilityZone::Name>",
            "List<AWS::EC2::Image::Id>",
            "List<AWS::EC2::Instance::Id>",
            "List<AWS::EC2::KeyPair::KeyName>",
            "List<AWS::EC2::SecurityGroup::GroupName>",
            "List<AWS::EC2::SecurityGroup::Id>",
            "List<AWS::EC2::Subnet::Id>",
            "List<AWS::EC2::Volume::Id>",
            "List<AWS::EC2::VPC::Id>",
            "List<AWS::Route53::HostedZone::Id>",

            // Validated Resource Types from Parameter Store
            "AWS::SSM::Parameter::Value<AWS::EC2::AvailabilityZone::Name>",
            "AWS::SSM::Parameter::Value<AWS::EC2::Image::Id>",
            "AWS::SSM::Parameter::Value<AWS::EC2::Instance::Id>",
            "AWS::SSM::Parameter::Value<AWS::EC2::KeyPair::KeyName>",
            "AWS::SSM::Parameter::Value<AWS::EC2::SecurityGroup::GroupName>",
            "AWS::SSM::Parameter::Value<AWS::EC2::SecurityGroup::Id>",
            "AWS::SSM::Parameter::Value<AWS::EC2::Subnet::Id>",
            "AWS::SSM::Parameter::Value<AWS::EC2::Volume::Id>",
            "AWS::SSM::Parameter::Value<AWS::EC2::VPC::Id>",
            "AWS::SSM::Parameter::Value<AWS::Route53::HostedZone::Id>",

            // Validated List of Resource Types from Parameter Store
            "AWS::SSM::Parameter::Value<List<AWS::EC2::AvailabilityZone::Name>",
            "AWS::SSM::Parameter::Value<List<AWS::EC2::Image::Id>",
            "AWS::SSM::Parameter::Value<List<AWS::EC2::Instance::Id>",
            "AWS::SSM::Parameter::Value<List<AWS::EC2::KeyPair::KeyName>",
            "AWS::SSM::Parameter::Value<List<AWS::EC2::SecurityGroup::GroupName>",
            "AWS::SSM::Parameter::Value<List<AWS::EC2::SecurityGroup::Id>",
            "AWS::SSM::Parameter::Value<List<AWS::EC2::Subnet::Id>",
            "AWS::SSM::Parameter::Value<List<AWS::EC2::Volume::Id>",
            "AWS::SSM::Parameter::Value<List<AWS::EC2::VPC::Id>",
            "AWS::SSM::Parameter::Value<List<AWS::Route53::HostedZone::Id>"
        };

        //--- Class Methods ---
        private static bool IsValidCloudFormationParameterType(string type) => _cloudFormationParameterTypes.Contains(type);

        //--- Constructors ---
        public ParameterDeclarationValidator(IModuleValidatorDependencyProvider provider) {
            Provider = provider ?? throw new System.ArgumentNullException(nameof(provider));
        }

        //--- Properties ---
        private IModuleValidatorDependencyProvider Provider { get; }
        private ILogger Logger => Provider.Logger;

        //--- Methods ---
        public void Validate(ModuleDeclaration moduleDeclaration) {
            moduleDeclaration.InspectNode(node => {
                if(node is ParameterDeclaration parameterDeclaration) {
                    ValidateParameter(parameterDeclaration);
                }
            });
        }

        private void ValidateParameter(ParameterDeclaration node) {
            ValidateParameterStructure(node);
            ValidateParemeterType(node);
            ValidateParameterAllow(node);
        }

        private void ValidateParameterStructure(ParameterDeclaration node) {

            // ensure parameter declaration is a child of the module declaration (nesting is not allowed)
            if(!(node.Parents.OfType<ADeclaration>().FirstOrDefault() is ModuleDeclaration)) {
                Logger.Log(ParameterDeclarationCannotBeNested, node);
            }
        }

        private void ValidateParemeterType(ParameterDeclaration node) {

            // default 'Type' attribute value is 'String' when omitted
            if(node.Type == null) {

                // TODO: should we really set this, thus modifying the original? or issue a warning
                node.Type = Fn.Literal("String");
            } else if(
                !IsValidCloudFormationParameterType(node.Type.Value)
                && !Provider.IsValidResourceType(node.Type.Value)
            ) {
                Logger.Log(UnknownType(node.Type.Value), node.Type);
            }

            // default 'Section' attribute value is "Module Settings" when omitted
            if(node.Section == null) {
                node.Section = Fn.Literal("Module Settings");
            }

            // the 'Description' attribute cannot exceed 4,000 characters
            if((node.Description != null) && (node.Description.Value.Length > MAX_PARAMETER_DESCRIPTION_LENGTH)) {
                Logger.Log(DescriptionAttributeExceedsSizeLimit, node.Description);
            }

            // only the 'Number' type can have the 'MinValue' and 'MaxValue' attributes
            if(node.Type.Value == "Number") {
                if((node.MinValue != null) && !int.TryParse(node.MinValue.Value, out var _)) {
                    Logger.Log(MinValueMustBeAnInteger, node.MinValue);
                }
                if((node.MaxValue != null) && !int.TryParse(node.MaxValue.Value, out var _)) {
                    Logger.Log(MaxValueMustBeAnInteger, node.MaxValue);
                }
                if(
                    (node.MinValue != null)
                    && (node.MaxLength != null)
                    && int.TryParse(node.MinValue.Value, out var minValueRange)
                    && int.TryParse(node.MaxLength.Value, out var maxValueRange)
                    && (maxValueRange < minValueRange)
                ) {
                    Logger.Log(MinMaxValueInvalidRange, node.MinValue);
                }

            } else {

                // ensure Number parameter options are not used
                if(node.MinValue != null) {
                    Logger.Log(MinValueAttributeRequiresNumberType, node.MinValue);
                }
                if(node.MaxValue != null) {
                    Logger.Log(MaxValueAttributeRequiresNumberType, node.MaxValue);
                }
            }

            // only the 'String' type can have the 'AllowedPattern', 'MinLength', and 'MaxLength' attributes
            if(node.Type.Value == "String") {

                // the 'AllowedPattern' attribute must be a valid regex expression
                if(node.AllowedPattern != null) {

                    // check if 'AllowedPattern' is a valid regular expression
                    try {
                        new Regex(node.AllowedPattern.Value);
                    } catch {
                        Logger.Log(AllowedPatternAttributeInvalid, node.AllowedPattern);
                    }
                } else if(node.ConstraintDescription != null) {

                    // the 'ConstraintDescription' attribute is only valid in conjunction with the 'AllowedPattern' attribute
                    Logger.Log(ConstraintDescriptionAttributeRequiresAllowedPatternAttribute, node.ConstraintDescription);
                }
                if(node.MinLength != null) {
                    if(!int.TryParse(node.MinLength.Value, out var minLength)) {
                        Logger.Log(MinLengthMustBeAnInteger, node.MinLength);
                    } else if(minLength < 0) {
                        Logger.Log(MinLengthMustBeNonNegative, node.MinLength);
                    } else if(minLength > MAX_PARAMETER_VALUE_LENGTH) {
                        Logger.Log(MinLengthTooLarge, node.MinLength);
                    }
                }
                if(node.MaxLength != null) {
                    if(!int.TryParse(node.MaxLength.Value, out var maxLength)) {
                        Logger.Log(MaxLengthMustBeAnInteger, node.MaxLength);
                    } else if(maxLength <= 0) {
                        Logger.Log(MaxLengthMustBePositive, node.MaxLength);
                    } else if(maxLength > MAX_PARAMETER_VALUE_LENGTH) {
                        Logger.Log(MaxLengthTooLarge, node.MaxLength);
                    }
                }
                if(
                    (node.MinLength != null)
                    && (node.MaxLength != null)
                    && int.TryParse(node.MinLength.Value, out var minLengthRange)
                    && int.TryParse(node.MaxLength.Value, out var maxLengthRange)
                    && (maxLengthRange < minLengthRange)
                ) {
                    Logger.Log(MinMaxLengthInvalidRange, node.MinLength);
                }
            } else {

                // ensure String parameter options are not used
                if(node.AllowedPattern != null) {
                    Logger.Log(AllowedPatternAttributeRequiresStringType, node.AllowedPattern);
                }
                if(node.ConstraintDescription != null) {
                    Logger.Log(ConstraintDescriptionAttributeRequiresStringType, node.ConstraintDescription);
                }
                if(node.MinLength != null) {
                    Logger.Log(MinLengthAttributeRequiresStringType, node.MinLength);
                }
                if(node.MaxLength != null) {
                    Logger.Log(MaxLengthAttributeRequiresStringType, node.MaxLength);
                }
            }

            // only the 'Secret' type can have the 'EncryptionContext' attribute
            if(node.Type.Value == "Secret") {

                // all 'EncryptionContext' values must be literal values
                if(node.EncryptionContext != null) {

                    // all expressions must be literals for the EncryptionContext
                    foreach(var kv in node.EncryptionContext) {
                        if(!(kv.Value is LiteralExpression)) {
                            Logger.Log(EncryptionContextExpectedLiteralStringExpression, kv.Value);
                        }
                    }
                }
            } else {

                // ensure Secret parameter options are not used
                if(node.EncryptionContext != null) {
                    Logger.Log(EncryptionContextAttributeRequiresSecretType, node.EncryptionContext);
                }
            }
        }

        private void ValidateParameterAllow(ParameterDeclaration node) {

            // TODO: generalize this to IAllowDeclaration

            if(node.Allow == null) {

                // nothing to validate
                return;
            }
            if(node.Type == null) {
                Logger.Log(Error.AllowAttributeRequiresTypeAttribute, node);
            } else if(node.Type.Value == "AWS") {

                // nothing to do; any 'Allow' expression is legal with 'AWS' type
            } else if(!Provider.IsValidResourceType(node.Type.Value)) {
                Logger.Log(AllowAttributeRequiresCloudFormationType, node);
            } else {

                // TODO: check if the allowed operations are valid for the specified type
            }
        }
    }
}