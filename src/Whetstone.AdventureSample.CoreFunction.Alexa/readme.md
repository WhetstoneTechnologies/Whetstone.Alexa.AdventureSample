

netsh http add urlacl url=https://whetstone.ngrok.io:7071/ user=everyone

netsh http delete urlacl url=http://localhost:7071/


ngrok http 7071 -host-header="localhost:7071" -subdomain=whetstone