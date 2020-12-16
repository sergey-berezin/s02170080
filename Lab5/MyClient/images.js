let image = document.getElementById('image')
let cur = 0
let imagesArray

async function selectionChanged(sel)
{
	imagesArray = new Array()
	let className = sel.options[sel.selectedIndex].value
	if (className == '') 
	{
		image.src = ''
		document.getElementById('buttons').style = 'visibility: hidden'
		return
	}
	let response = await fetch('https://localhost:5001/images/' + className)
	let json = await response.json()
	
	for (let i = 0; i < json.length; i++) {
		const bytes = atob(json[i].blob)
		const byteCodes = new Array(bytes.length)
		for (let i = 0; i < bytes.length; i++) {
			byteCodes[i] = bytes.charCodeAt(i)
		}
		const byteArray = new Uint8Array(byteCodes)
		const objectURL = URL.createObjectURL(new Blob([byteArray]))
		imagesArray.push(objectURL)
	}

	image.src = imagesArray[0]
	cur = 0
	document.getElementById('buttons').style = 'visibility: visible'
}

function prev() {
	if (cur == 0)
		cur = imagesArray.length - 1
	else
		--cur
    image.src = imagesArray[cur]
}

function next() {
    if (cur == imagesArray.length - 1)
        cur = 0
	else
		++cur
    image.src = imagesArray[cur]
}