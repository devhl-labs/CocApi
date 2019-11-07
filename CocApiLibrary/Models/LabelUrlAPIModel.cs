using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocApiLibrary.Models
{
    public class LabelUrlApiModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Medium { get; set; } = string.Empty;

        public string Small { get; set; } = string.Empty;

        ///// <summary>
        ///// The Api returns the wrong url for the lables.  Use the corrected url until the Api is updated.
        ///// </summary>
        //public string MediumCorrected { get; set; } = string.Empty;


        ///// <summary>
        ///// The Api returns the wrong url for the lables.  Use the corrected url until the Api is updated.
        ///// </summary>
        //public string SmallCorrected { get; set; } = string.Empty;

        //private string GetCorrectedURL(string badUrl)
        //{

        //    //village labels
        //    if (badUrl.Contains("ZxJp9606Vl1sa0GHg5JmGp8TdHS4l0jE4WFuil1ENvA.png")) return badUrl.Replace("ZxJp9606Vl1sa0GHg5JmGp8TdHS4l0jE4WFuil1ENvA.png", "JOzAO4r91eVaJELAPB-iuAx6f_zBbRPCLM_ag5mpK4s.png");  // clan wars
        //    if (badUrl.Contains("JOzAO4r91eVaJELAPB-iuAx6f_zBbRPCLM_ag5mpK4s.png")) return badUrl.Replace("JOzAO4r91eVaJELAPB-iuAx6f_zBbRPCLM_ag5mpK4s.png", "tINt65InVEc35rFYkxqFQqGDTsBpVRqY9K7BJf5kr4A.png");  // clan war league
        //    if (badUrl.Contains("tINt65InVEc35rFYkxqFQqGDTsBpVRqY9K7BJf5kr4A.png")) return badUrl.Replace("tINt65InVEc35rFYkxqFQqGDTsBpVRqY9K7BJf5kr4A.png", "LIXkluJJeg4ATNVQgO6scLheXxmNpyBLRYGldtv-Miw.png");  // trophy pushing
        //    if (badUrl.Contains("L1JDFhgOJyt1jcNnb6-IkBddd9vQSn2UeoQQGjVLEYI.png")) return badUrl.Replace("L1JDFhgOJyt1jcNnb6-IkBddd9vQSn2UeoQQGjVLEYI.png", "UEjY-kAdKcE6bPfI_X1L4s-ADYI_IJLuxx5cmClykdU.png");  // friendly wars
        //    if (badUrl.Contains("LIXkluJJeg4ATNVQgO6scLheXxmNpyBLRYGldtv-Miw.png")) return badUrl.Replace("LIXkluJJeg4ATNVQgO6scLheXxmNpyBLRYGldtv-Miw.png", "ZxJp9606Vl1sa0GHg5JmGp8TdHS4l0jE4WFuil1ENvA.png");  // clan games
        //    if (badUrl.Contains("UEjY-kAdKcE6bPfI_X1L4s-ADYI_IJLuxx5cmClykdU.png")) return badUrl.Replace("UEjY-kAdKcE6bPfI_X1L4s-ADYI_IJLuxx5cmClykdU.png", "8Q08M2dj1xz1Zx-sAre6QO14hOX2aiEvg-FaGGSX-7M.png");  // builder base
        //    if (badUrl.Contains("gwTgG4oOwkse3eCpFL05AFArJMmMULIlecXNrl1Mv2g.png")) return badUrl.Replace("gwTgG4oOwkse3eCpFL05AFArJMmMULIlecXNrl1Mv2g.png", "PcgplBTQo2W_PXYqMi0i6g6nrNMjzCM8Ipd_umSnuHw.png");  // base design
        //    if (badUrl.Contains("aKHRoHhkn6n3wj09tWxAA3DfKL6s45dHe3_VtKgkkhQ.png")) return badUrl.Replace("aKHRoHhkn6n3wj09tWxAA3DfKL6s45dHe3_VtKgkkhQ.png", "L1JDFhgOJyt1jcNnb6-IkBddd9vQSn2UeoQQGjVLEYI.png");  // farming
        //    if (badUrl.Contains("MvL0LDt0yv9AI-Vevpu8yE5NAJUIV05Ofpsr4IfGRxQ.png")) return badUrl.Replace("MvL0LDt0yv9AI-Vevpu8yE5NAJUIV05Ofpsr4IfGRxQ.png", "mcWhk0ii7CyjiiHOidhRofrSulpVrxjDu24cQtGCQbE.png");  // active donator
        //    if (badUrl.Contains("mcWhk0ii7CyjiiHOidhRofrSulpVrxjDu24cQtGCQbE.png")) return badUrl.Replace("mcWhk0ii7CyjiiHOidhRofrSulpVrxjDu24cQtGCQbE.png", "jEvZf9PnfPaqYh2PMLBoJfB1BoBpomerqmsYWDYisKY.png");  // active daily
        //    if (badUrl.Contains("jEvZf9PnfPaqYh2PMLBoJfB1BoBpomerqmsYWDYisKY.png")) return badUrl.Replace("jEvZf9PnfPaqYh2PMLBoJfB1BoBpomerqmsYWDYisKY.png", "H75LWbZqe5Lm2rXYUrEDgQNa3kpZdtFCjiyvnNSvh00.png");  // hungry learner
        //    if (badUrl.Contains("t0KZ4173i9vJFrD5F06-2TFNFk9UwJXxPjfutcG-dig.png")) return badUrl.Replace("t0KZ4173i9vJFrD5F06-2TFNFk9UwJXxPjfutcG-dig.png", "aKHRoHhkn6n3wj09tWxAA3DfKL6s45dHe3_VtKgkkhQ.png");  // frienldy
        //    if (badUrl.Contains("H75LWbZqe5Lm2rXYUrEDgQNa3kpZdtFCjiyvnNSvh00.png")) return badUrl.Replace("H75LWbZqe5Lm2rXYUrEDgQNa3kpZdtFCjiyvnNSvh00.png", "MvL0LDt0yv9AI-Vevpu8yE5NAJUIV05Ofpsr4IfGRxQ.png");  // talkative
        //    //if (badUrl.Contains("sy5nJmT4BFjS4iT4_iILE02rfrO8VjgpGKFE0rLmot4.png")) return badUrl.Replace("sy5nJmT4BFjS4iT4_iILE02rfrO8VjgpGKFE0rLmot4.png", "");                                               // teacher
        //    if (badUrl.Contains("DfTPKAsvjdsD-CFfbpmfIJiT2uF3FQLfftRdJgBA37Y.png")) return badUrl.Replace("DfTPKAsvjdsD-CFfbpmfIJiT2uF3FQLfftRdJgBA37Y.png", "gwTgG4oOwkse3eCpFL05AFArJMmMULIlecXNrl1Mv2g.png");  // competitive
        //    if (badUrl.Contains("u-VKK5y0hj0U8B1xdawjxNcXciv-fwMK3VqEBWCn1oM.png")) return badUrl.Replace("u-VKK5y0hj0U8B1xdawjxNcXciv-fwMK3VqEBWCn1oM.png", "DfTPKAsvjdsD-CFfbpmfIJiT2uF3FQLfftRdJgBA37Y.png");  // veteran
        //    if (badUrl.Contains("PcgplBTQo2W_PXYqMi0i6g6nrNMjzCM8Ipd_umSnuHw.png")) return badUrl.Replace("PcgplBTQo2W_PXYqMi0i6g6nrNMjzCM8Ipd_umSnuHw.png", "u-VKK5y0hj0U8B1xdawjxNcXciv-fwMK3VqEBWCn1oM.png");  // newbie
        //    if (badUrl.Contains("8Q08M2dj1xz1Zx-sAre6QO14hOX2aiEvg-FaGGSX-7M.png")) return badUrl.Replace("8Q08M2dj1xz1Zx-sAre6QO14hOX2aiEvg-FaGGSX-7M.png", "t0KZ4173i9vJFrD5F06-2TFNFk9UwJXxPjfutcG-dig.png");  // amateur attacker

        //    //clan labels
        //    if (badUrl.Contains("lXaIuoTlfoNOY5fKcQGeT57apz1KFWkN9-raxqIlMbE.png")) return badUrl.Replace("lXaIuoTlfoNOY5fKcQGeT57apz1KFWkN9-raxqIlMbE.png", "5w60_3bdtYUe9SM6rkxBRyV_8VvWw_jTlDS5ieU3IsI.png");  // clan wars
        //    if (badUrl.Contains("5w60_3bdtYUe9SM6rkxBRyV_8VvWw_jTlDS5ieU3IsI.png")) return badUrl.Replace("5w60_3bdtYUe9SM6rkxBRyV_8VvWw_jTlDS5ieU3IsI.png", "hNtigjuwJjs6PWhVtVt5HvJgAp4ZOMO8e2nyjHX29sA.png");  // clan war league
        //    if (badUrl.Contains("hNtigjuwJjs6PWhVtVt5HvJgAp4ZOMO8e2nyjHX29sA.png")) return badUrl.Replace("hNtigjuwJjs6PWhVtVt5HvJgAp4ZOMO8e2nyjHX29sA.png", "7qU7tQGERiVITVG0CPFov1-BnFldu4bMN2gXML5bLIU.png");  // trophy pushing
        //    if (badUrl.Contains("6NxZMDn9ryFw8-FHJJimcEkKwnXZHMVUp_0cCVT6onY.png")) return badUrl.Replace("6NxZMDn9ryFw8-FHJJimcEkKwnXZHMVUp_0cCVT6onY.png", "kyuaiAWdnD9v3ReYPS3_x6QP3V3e0nNAPyDroOIDFZQ.png");  // friendly wars
        //    if (badUrl.Contains("7qU7tQGERiVITVG0CPFov1-BnFldu4bMN2gXML5bLIU.png")) return badUrl.Replace("7qU7tQGERiVITVG0CPFov1-BnFldu4bMN2gXML5bLIU.png", "lXaIuoTlfoNOY5fKcQGeT57apz1KFWkN9-raxqIlMbE.png");  // clan games
        //    if (badUrl.Contains("kyuaiAWdnD9v3ReYPS3_x6QP3V3e0nNAPyDroOIDFZQ.png")) return badUrl.Replace("kyuaiAWdnD9v3ReYPS3_x6QP3V3e0nNAPyDroOIDFZQ.png", "3oOuYkPdkjWVrBUITgByz9Ur0nmJ4GsERXc-1NUrjKg.png");  // builder base
        //    if (badUrl.Contains("LG966XuC6YoEJsPthcgtyJ8uS46LqYDAeiHJNQKR3YQ.png")) return badUrl.Replace("LG966XuC6YoEJsPthcgtyJ8uS46LqYDAeiHJNQKR3YQ.png", "DhBE-1SSnrZQtsfjVHyNW-BTBWMc8Zoo34MNRCNiRsA.png");  // base design
        //    if (badUrl.Contains("zyaTKuJXrsPiU3DvjgdqaSA6B1qvcQ0cjD6ktRah4xs.png")) return badUrl.Replace("zyaTKuJXrsPiU3DvjgdqaSA6B1qvcQ0cjD6ktRah4xs.png", "T1c8AYalTn_RruVkY0mRPwNYF5n802thTBEEnOtNTMw.png");  // international
        //    if (badUrl.Contains("iLWz6AiaIHg_DqfG6s9vAxUJKb-RsPbSYl_S0ii9GAM.png")) return badUrl.Replace("iLWz6AiaIHg_DqfG6s9vAxUJKb-RsPbSYl_S0ii9GAM.png", "6NxZMDn9ryFw8-FHJJimcEkKwnXZHMVUp_0cCVT6onY.png");  // farming
        //    if (badUrl.Contains("RauzS-02tv4vWm1edZ-q3gPQGWKGANLZ-85HCw_NVP0.png")) return badUrl.Replace("RauzS-02tv4vWm1edZ-q3gPQGWKGANLZ-85HCw_NVP0.png", "ImSgCg88EEl80mwzFZMIiJTqa33bJmJPcl4v2eT6O04.png");  // donations
        //    if (badUrl.Contains("hM7SHnN0x7syFa-s6fE7LzeO5yWG2sfFpZUHuzgMwQg.png")) return badUrl.Replace("hM7SHnN0x7syFa-s6fE7LzeO5yWG2sfFpZUHuzgMwQg.png", "zyaTKuJXrsPiU3DvjgdqaSA6B1qvcQ0cjD6ktRah4xs.png");  // friendly
        //    if (badUrl.Contains("T1c8AYalTn_RruVkY0mRPwNYF5n802thTBEEnOtNTMw.png")) return badUrl.Replace("T1c8AYalTn_RruVkY0mRPwNYF5n802thTBEEnOtNTMw.png", "iLWz6AiaIHg_DqfG6s9vAxUJKb-RsPbSYl_S0ii9GAM.png");  // talkative
        //    if (badUrl.Contains("ImSgCg88EEl80mwzFZMIiJTqa33bJmJPcl4v2eT6O04.png")) return badUrl.Replace("ImSgCg88EEl80mwzFZMIiJTqa33bJmJPcl4v2eT6O04.png", "Kv1MZQfd5A7DLwf1Zw3tOaUiwQHGMwmRpjZqOalu_hI.png");  // underdog
        //    if (badUrl.Contains("Kv1MZQfd5A7DLwf1Zw3tOaUiwQHGMwmRpjZqOalu_hI.png")) return badUrl.Replace("Kv1MZQfd5A7DLwf1Zw3tOaUiwQHGMwmRpjZqOalu_hI.png", "RauzS-02tv4vWm1edZ-q3gPQGWKGANLZ-85HCw_NVP0.png");  // relaxed
        //    if (badUrl.Contains("DhBE-1SSnrZQtsfjVHyNW-BTBWMc8Zoo34MNRCNiRsA.png")) return badUrl.Replace("DhBE-1SSnrZQtsfjVHyNW-BTBWMc8Zoo34MNRCNiRsA.png", "LG966XuC6YoEJsPthcgtyJ8uS46LqYDAeiHJNQKR3YQ.png");  // competitive
        //    if (badUrl.Contains("3oOuYkPdkjWVrBUITgByz9Ur0nmJ4GsERXc-1NUrjKg.png")) return badUrl.Replace("3oOuYkPdkjWVrBUITgByz9Ur0nmJ4GsERXc-1NUrjKg.png", "hM7SHnN0x7syFa-s6fE7LzeO5yWG2sfFpZUHuzgMwQg.png");  // newbie friendly

        //    return badUrl;
        //}
        
    }
}
