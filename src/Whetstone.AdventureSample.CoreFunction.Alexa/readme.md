

netsh http add urlacl url=https://whetstone.ngrok.io:7071/ user=everyone

netsh http delete urlacl url=http://localhost:7071/


ngrok http 7071 -host-header="localhost:7071" -subdomain=whetstone



Public Alexa endpoint:
https://adventuresamplefunction.azurewebsites.net/api/AlexaFunction?code=W/X2R85cERJTRpVssPUazy/CWDuSVR/LbG3T7Z1a4VFI356byFAuFw==