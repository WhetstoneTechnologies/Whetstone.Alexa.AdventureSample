# SpaceFacts -- Module 5 - Prepare for Production

## Introduction

The Space Fact Skill is not yet ready to submit for publication on Alexa. This modules discusses the additional requirements to fulfill before a skill can be submitted to Alexa for review.

### Validate the Alexa Request

When hosting a REST API that services Alexa requests, Amazon requires the service to validate signed calls using certificates. For more information about this process, see [Host a Custom Skill as a Web Service](https://developer.amazon.com/docs/custom-skills/host-a-custom-skill-as-a-web-service.html) in Alexa documentation.

To add certificate verification logic to the Alexa Space Facts skill, add the following code to the Run method toward the start of the method. The compiler directive ensures this runs only in release mode. 

``` C#
#if !DEBUG
    IAlexaRequestVerifier reqVerifier = new AlexaCertificateVerifier();
    bool isValid = false;

    try
    {
        isValid = await reqVerifier.IsCertificateValidAsync(req);
    }
    catch (Exception ex)
    {
        log.LogError(ex, "Error processing certificate");

    }

    if (!isValid)
        return new BadRequestResult();

#endif
``` 

### Fill out the Distribution Form

This distribution form drives when appears in the public listing of the Alexa Skill. Fill this out, including key words and icons. Key words are used when applying nameless invocation.

 <img src="/docs/images/SkillPublish01.png?raw=true" width="50%"/>

[Previous Module](/docs/spacefactstutorial/SpaceFactsTutorial04.md) | [Next Module](/docs/spacefactstutorial/SpaceFactsTutorial06.md)