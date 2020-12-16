$(async () => 
{
    try 
	{
        let response = await fetch('https://localhost:5001/images')
        let json = await response.json()

        let list = document.getElementById('classesList')
        for (let i in json) 
		{
			let opt = document.createElement('option')
			opt.value = json[i].className
			opt.innerHTML = json[i].className + ' ' + json[i].count
			list.appendChild(opt)
        }
    }
    catch (e) 
	{
        console.log(e)
    }
})