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

using System.Threading.Tasks;
using Amazon.Lambda.Core;
using LambdaSharp;
using LambdaSharp.SimpleNotificationService;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(LambdaSharp.Serialization.LambdaJsonSerializer))]

namespace SnsSample.MyFunction {

    public class MyMessage {

        //--- Properties ---
        public string Text { get; set; }
    }

    public class Function : ALambdaTopicFunction<MyMessage> {

        //--- Methods ---
        public override Task InitializeAsync(LambdaConfig config)
            => Task.CompletedTask;

        public override async Task ProcessMessageAsync(MyMessage message) {
            LogInfo($"Message.Text = {message.Text}");
            LogInfo($"CurrentRecord.Message = {CurrentRecord.Message}");
            LogInfo($"CurrentRecord.MessageAttributes = {CurrentRecord.MessageAttributes}");
            foreach(var attribute in CurrentRecord.MessageAttributes) {
                LogInfo($"CurrentRecord.MessageAttributes.{attribute.Key} = {attribute.Value}");
            }
            LogInfo($"CurrentRecord.MessageId = {CurrentRecord.MessageId}");
            LogInfo($"CurrentRecord.Signature = {CurrentRecord.Signature}");
            LogInfo($"CurrentRecord.SignatureVersion = {CurrentRecord.SignatureVersion}");
            LogInfo($"CurrentRecord.SigningCertUrl = {CurrentRecord.SigningCertUrl}");
            LogInfo($"CurrentRecord.Subject = {CurrentRecord.Subject}");
            LogInfo($"CurrentRecord.Timestamp = {CurrentRecord.Timestamp}");
            LogInfo($"CurrentRecord.TopicArn = {CurrentRecord.TopicArn}");
            LogInfo($"CurrentRecord.Type = {CurrentRecord.Type}");
            LogInfo($"CurrentRecord.UnsubscribeUrl = {CurrentRecord.UnsubscribeUrl}");
        }
    }
}