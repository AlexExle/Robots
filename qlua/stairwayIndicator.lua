Settings=
{
	Name = "StairwayIndicator",
	step = 1000,
	offset = 35,
	line =
	{
		{
		Name = "Stairway",
		Color = RGB(255, 0, 0),
		Type = TYPE_LINE,
		Width = 1
		}
	}
}

currentPoint = 0
lowPoint = 0
highPoint = 0

function Init()

	return 1
end

function OnCalculate(index)
	if index < 1 then
		currentPoint = O(index)
		currentPoint = currentPoint - (currentPoint % Settings.step) + Settings.offset
		lowPoint = currentPoint - Settings.step
		highPoint = currentPoint + Settings.step
	else
		if H(index) >= highPoint then
		   currentPoint = highPoint
		   lowPoint =  highPoint - Settings.step
		   highPoint = highPoint + Settings.step
		elseif L(index) <= lowPoint then
			 currentPoint = lowPoint
			 highPoint = lowPoint + Settings.step
			 lowPoint = lowPoint - Settings.step
		end
	end
	return currentPoint;
end
