using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VCAuthn.Models;

namespace VCAuthn.Utils
{
    public static class PresentationRequestUtils
    {
        public static string GeneratePresentationRequest(PresentationRequestConfiguration configuration)
        {
            PresentationRequest_v_1_0 presentationRequest_1_0 = new PresentationRequest_v_1_0()
            {
                Version = configuration.Version,
                Name = configuration.Name
            };

            foreach (RequestedAttribute reqAttribute in configuration.RequestedAttributes)
            {
                presentationRequest_1_0.RequestedAttributes.Add(Guid.NewGuid().ToString(), reqAttribute);
            }

            foreach (RequestedPredicate reqPredicate in configuration.RequestedPredicates)
            {
                presentationRequest_1_0.RequestedPredicates.Add(Guid.NewGuid().ToString(), reqPredicate);
            }

            Dictionary<string, PresentationRequest_v_1_0> requestBody = new Dictionary<string, PresentationRequest_v_1_0>()
            {
                {"proof_request", presentationRequest_1_0}
            };
            return JsonConvert.SerializeObject(requestBody);
        }

        public static List<PresentationAttachment> GeneratePresentationAttachments(PresentationRequest presentationRequest)
        {
            string base64Payload = presentationRequest.ToJson().ToBase64();
            PresentationAttachment attachment = new PresentationAttachment()
            {
                Id = "libindy-request-presentation-0",
                MimeType = "application/json",
                Data = new Dictionary<string, string>()
                {
                    {"base64", base64Payload}
                },
            };

            return new List<PresentationAttachment>()
            {
                attachment
            };
        }

        /// <summary> Utility function that extracts the first Indy <c>Presentationrequest</c> object found
        /// in the provided list of <c>PresentationAttachment</c> objects. The Indy <c>Presentationrequest</c>
        /// is identified by <c>"@id": "libindy-request-presentation-0"</c>, as specified in the Aries RFC 0037.
        /// <see cref="https://github.com/hyperledger/aries-rfcs/tree/master/features/0037-present-proof#request-presentation"/> 
        /// </summary>
        public static PresentationRequest ExtractIndyPresentationPequest(List<PresentationAttachment> presentationAttachments)
        {
            PresentationRequest presentationRequest = null;

            presentationAttachments.ForEach(delegate(PresentationAttachment attachment)
            {
                if (attachment.Id.Equals("libindy-request-presentation-0"))
                {
                    // found first indy presentation attachment, deserialize it so thatit can be returned
                    presentationRequest = attachment.Data["base64"].toPresentationRequestObject();
                }
            });

            return presentationRequest;
        }

        public static PresentationRequest toPresentationRequestObject(this string attachmentBase64Data)
        {
            return JsonConvert.DeserializeObject<PresentationRequest>(attachmentBase64Data.FromBase64());
        }
    }


}