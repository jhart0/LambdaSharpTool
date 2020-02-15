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

#nullable disable

using System.Collections.Generic;

namespace LambdaSharp.Tool.Model {

    public class CloudFormationSpec {

        // SEE: https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/cfn-resource-specification-format.html

        //--- Properties ---
        public string ResourceSpecificationVersion { get; set; }
        public IDictionary<string, ResourceType> ResourceTypes { get; set; }
        public IDictionary<string, ResourceType> PropertyTypes { get; set; }
    }

    public class ResourceType {

        //--- Properties ---
        public string Documentation { get; set; }
        public IDictionary<string, AttributeType> Attributes { get; set; }
        public IDictionary<string, PropertyType> Properties { get; set; }
    }

    public class AttributeType {

        //--- Properties ---
        public string ItemType { get; set; }
        public string PrimitiveItemType { get; set; }
        public string PrimitiveType { get; set; }
        public string Type { get; set; }
    }

    public class PropertyType {

        //--- Properties ---

        public bool DuplicatesAllowed { get; set; }
        public string ItemType { get; set; }
        public string PrimitiveItemType { get; set; }
        public string PrimitiveType { get; set; }
        public bool Required { get; set; }
        public string Type { get; set; }
    }
}
