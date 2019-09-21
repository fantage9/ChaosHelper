	import requests

def getJson( poesessid ):
	url = 'https://www.pathofexile.com/character-window/get-stash-items?league=Legion&tabs=0&tabIndex=1&accountName=에스크로'
	payload = {'POESESSID' : poesessid}
	response = requests.post(url, cookies=payload)
	print(response.status_code)
	if response.status_code == 200:
	    return response
