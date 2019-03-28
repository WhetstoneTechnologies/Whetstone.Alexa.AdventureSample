

netsh http add urlacl url=https://whetstone.ngrok.io:54768/ user=everyone

netsh http delete urlacl url=http://localhost:54768/


ngrok http 54768 -host-header="localhost:54768" -subdomain=whetstone