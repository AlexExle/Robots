require "QL"
require 'luaxml'

VERSION='0.1'
log='full_BB.log'
set_path='settings.xml'
is_run=false

step = 0;
offset = 0;
quantityOfPos = 0;
priceStep = 0
clientcode = ""
account = ""
t = {}

empty_str=[[<table>
	<security value="SBER" />
	<step value="2" />
	<offset value="10" />
	<quantityOfPos value="5" />
	<priceStep value="3" />
	<account value='100101' />
	<clientcode value='100' />
	<graphname value='bb' />
</table>]]


function OnInitDo()

	toLog(log,"Try to open settings "..set_path)
	local f=io.open(set_path)
	if f==nil then f=io.open(set_path,"w") f:close()
	else f:close() end
	local file=xml.load(set_path)
	toLog(log,"XML loaded")
	if file==nil then
		--message("Begemot can`t open settings file!",3)
		toLog(log,"File can`t be openned! File would be created.")
		file=xml.eval(empty_str)
	end
	toLog(log,"File oppened")

	file:save(set_path)
	toLog(log,'Settings file------')
	toLog(log,file)
	toLog(log,'------------')

	security=file:find("security").value
	class=getSecurityInfo('',security).class_code
	minstep=getParam(security,"SEC_PRICE_STEP")
	volume=tonumber(file:find("step").value)
	slippage=tonumber(file:find("offset").value)*minstep
	take=tonumber(file:find("quantityOfPos").value)*minstep
	stop=tonumber(file:find("priceStep").value)*minstep
	acc=file:find("account").value
	clc=file:find("clientcode").value
	chart=file:find("graphname").value
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
	--toLog(log,'Main started')
    while is_run do
        sleep(50)
    end;
end

--как отлечить тестовые данные от не тестовых. смотреть время, если меньше текущего... но насколько меньше?

--как зафигачить параметры Робота. XML файл с настройками еба. обновлять по комбинации клавишь!!!!!!!!!!!!!!!!!!
