require "QL"
require 'luaxml'
VERSION='0.1'

is_run=false

--[[Robot Settings]]--
set_path='settings.xml'
log='full_BB.log'
security = ""
indicatorId = ""
quantityOfPos = 0
priceSlippageStep = 0
clientcode = ""
account = ""
t = {}

robotState = {"inPosition","inEntering","waitingOrderActivation"}

empty_str = [[<table>
				<security value="SBER" />
				<indicatorId value="atr1" />
				<quantityOfPos value="5" />
				<priceSlippageStep value="3" />
				<account value='100101' />
				<clientcode value='100' />
			</table>]]


function OnInitDo()

	toLog(log,"Try to open settings "..set_path)
	--load file with settings
	local f = io.open(set_path)
	if f==nil then f=io.open(set_path,"w") f:close()
	else f:close() end
	local file=xml.load(set_path)
	toLog(log,"XML loaded")
	if file==nil then
		--message("Atr Channel robot can`t open settings file!",3)
		toLog(log,"File can`t be openned! File would be created.")
		file=xml.eval(empty_str)
	end
	toLog(log,"File oppened")

	file:save(set_path)
	toLog(log,'Settings file------')
	toLog(log,file)
	toLog(log,'------------')

	security = file:find("security").value
	indicatorId = file:find("indicatorId").value
	quantityOfPos = tonumber(file:find("quantityOfPos").value)
	priceSlippageStep = tonumber(file:find("priceSlippageStep").value)
	clientcode = tonumber(file:find("clientcode").value)
	account = tonumber(file:find("account").value)

	toLog(log,"Settings loaded")

	t=QTable:new()
	-- ��������� 2 �������
	t:AddColumn("SEC",QTABLE_STRING_TYPE,45)
	t:AddColumn("QUANTITY",QTABLE_STRING_TYPE,30)
	t:AddColumn("PRICE",QTABLE_STRING_TYPE,30)
	-- ��������� �������� ��� �������
	t:SetCaption('TestTable')
	-- ���������� �������
	t:Show()
	-- ��������� ������ ������

	return true
end

function OnStop()

    is_run = false
end

function OnInit(path)
	-- берем тек значение индикатора
	-- инициализируем логи, вся херня
end

function UpdateOrders(sharesCount, price)
	--шарашим ордера
end

function OnOrder(order)
	-- заявка выполнена? переворот!
end

function OnAllTrade(trade)
	toLog(log,"OnTrade")
	line=t:AddLine()
	t:SetValue(line,"SEC",trade.sec_code)
	t:SetValue(line,"QUANTITY",trade.qty)
	t:SetValue(line,"PRICE",trade.price)
end

function OnTrade(trade)
	toLog(log,"OnTrade")
	line=t:AddLine()
	t:SetValue(line,"SEC",trade.sec_code)
	t:SetValue(line,"QUANTITY",trade.qty)
	t:SetValue(line,"PRICE",trade.price)
end


function main()
	is_run=OnInitDo()

	local candlesCount = getNumCandles(linesCount) 	-- �������� ���������� ������ �� �������
	local linesCount = getLinesCount(indicatorId) 	-- �������� ���������� ����� � �������
	toLog(log,"Candles : " .. candlesCount)
    toLog(log,"Lines :" .. linesCount)
	message("total ".. candlesCount .. " candles in " ..linesCount .. " lines", 3)
    while is_run do

        sleep(50)
    end;
end

--как отлечить тестовые данные от не тестовых. смотреть время, если меньше текущего... но насколько меньше?

--как зафигачить параметры Робота. XML файл с настройками еба. обновлять по комбинации клавишь!!!!!!!!!!!!!!!!!!
