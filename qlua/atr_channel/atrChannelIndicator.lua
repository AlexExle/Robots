Settings=
{
	Name = "AtrChannelInd",
	period = 1000,
	multiplier = 35,
	line =
	{
		{
		Name = "HighChannel",
		Color = RGB(255, 0, 0),
		Type = TYPE_LINE,
		Width = 1
		},
		{
		Name = "LowChannel",
		Type = TYPE_LINE,
		Width = 1
		}
	}
}

zeroPoint = 0

function Init()
	AtrChannel = cached_ATR()
	return 1
end

function OnCalculate(index)
	if T(index)["hour"] == 23 and T(index)["minute"] == 45 then
	zeroPoint = C(index)
	end
	local currAtr = AtrChannel(index, Settings.period)
	if zeroPoint > 0 then
		return zeroPoint + curAtr * 2 * Settings.multiplier, zeroPoint - curAtr * 2 * Settings.multiplier
	else
		return 0,0
	end
end

function summ(_start, _end, array)
	local sum = 0 
	for i = _start, _end do 
		sum = sum + array[i]
	end 
	return sum
end

function cached_ATR()
	local atrCache = {}
	local trCache = {}
	return function(index, period)
		-- Вычисляем текущий TR
		local curTr = H(index) - L(index)
		local curAtr = 0
		if index > 1 then
			local hc = math.abs(H(index) - C(index-1))
			local lc = math.abs(L(index) - C(index-1))
			
			if curTr < hc then curTr = hc end
			if curTr < lc then curTr = lc end
		end
		trCache[index] = curTr
		if index < period then
			atrCache[index] = 0
		end
		if index == period then
			atrCache[index] = summ(1, period, trCache)/period
		end
		if index > period then
			atrCache[index] = (atrCache[index - 1]*(period-1) + trCache[index])/period
		end
		return atrCache[index]
	end
end
