


--------------------------------RemoveInstalledItem------------------------------------

function SetRemoveInstalledItem(work) 
	work.OnWork:Add("OnWork_InstalledItem")
	work.OnComplete:Add("OnComplete_AnyPre")
	work.OnComplete:Add("OnComplete_RemoveInstalledItem")
	work.OnComplete:Add("OnComplete_AnyPost")
	work.IsComplete = "IsComplete_RemoveInstalledItem"
	work.IsReady = "IsReady_RemoveInstalledItem"
	work.description = "RemoveInstalledItem " 


	

end

function IsReady_RemoveInstalledItem(work)	
	return true
end

function OnComplete_RemoveInstalledItem(work)
	--World.dbug("OnComplete_InstalledItemWork: " .. work:ToString())
	work.workTile.installedItem:Deconstruct()

end

function IsComplete_RemoveInstalledItem(work)
	--World.dbug("IsComplete_InstalledItemWork: " .. work:ToString())
	if work.workTime <= 0 then
		return true
	else
		return false
	end


end

--------------------------------InstalledItem------------------------------------
function SetInstalledItem(work,itemName) 
	
	work.canWorkFromNeighbourTiles = true
	proto = InstalledItem.GetPrototype(itemName)

	xfrom = work.workTile.world_x
	xto = xfrom + proto.width-1

	yfrom = work.workTile.world_y
	yto = yfrom + proto.height-1


	for x = xfrom, xto, 1 do
		for y = yfrom, yto, 1 do
			tile = World.current:GetTileAt(x,y)
			if (tile ~= nil) then
				World.dbug(x .. "," .. y .. " = " .. proto.type)
				work.relatedTiles:Add(tile)
				tile:AddWork(work)
			end
		end
	end

	if (proto ~= nil) then
		work.installedItemProto = proto
		work.recipe = proto.recipe
		work.workTime = proto.recipe.buildTime

		for kvp in luanet.each(work.recipe.resources) do
			resource = kvp.Value
			rName = resource.name
			rQty = resource.qtyRequired
			--[[
			World.dbug(tostring(rName .. " " .. rQty))
			World.dbug(resource.name .. " " .. resource.qtyRequired)


			]]--
			--World.dbug("k=" .. resource.Key .. ", v=" .. resource.Value:ToString())

			newItem = WorkItem.MakeWorkItem(work.workTile,"SetPrereq",work,rName,rQty)
			newItem.description = "description (" .. rName  .. ":" .. rQty .. ")"

			work:AddPrereq(newItem)

		end
		work.OnWork:Add("OnWork_InstalledItem")
		work.OnComplete:Add("OnComplete_AnyPre")
		work.OnComplete:Add("OnComplete_InstalledItem")
		work.OnComplete:Add("OnComplete_AnyPost")
		work.IsComplete = "IsComplete_InstalledItem"
		work.IsReady = "IsReady_InstalledItem"
		work.description = "InstalledItem " .. proto.type

		--[[
		for (string resourceName in j.recipe.resources.Keys) {
            //(Tile tile, Action<Job> cbJobComplete, Action<Job> cbJobCancelled, JOB_TYPE type, string description, Recipe recipe, string name) {
            Job nj = Job.MakeRecipeJob(j.tile, JOB_TYPE.HAUL, JOB_TYPE.HAUL.ToString(), j.recipe, resourceName, j);
            nj.cbLuaRegisterJobComplete("JobQueue_HaulJobComplete");
            nj.cbLuaRegisterJobCancelled("JobQueue_HaulJobCancelled");
            Add(nj, nj.priority);
            //publicJobs.Add(nj);


          }

		--  ]]

	end
end

---------------------------------------------PRE REQ
function SetPrereq(work,parent,invItemName,qty) 
	work.inventoryItemName = invItemName
	work.inventoryItemQtyRequired = qty
	work.inventoryItemQtyRemaining = qty
	work.parent = parent;
	work.inventory = parent.inventory
	work.inventorySearchStockpiles = true
	work.canWorkFromNeighbourTiles = true

	work.OnWork:Add("OnWork_PreReq")
	work.OnComplete:Add("OnComplete_AnyPre")
	work.OnComplete:Add("OnComplete_PreReq")
	work.OnComplete:Add("OnComplete_AnyPost")
	work.IsComplete = "IsComplete_PreReq"
	work.IsReady = "IsReady_PreReq"
	   	 
end

function OnWork_PreReq(work)

	work.assignedRobot:PlaceItemAtJob()

end

function OnComplete_AnyPre (work) 
	work.workTile:RemoveWork(work)
	
end

function OnComplete_AnyPost (work) 
	
	World.current.workManager:WorkItemComplete(work);
end

function IsComplete_PreReq(work) 
	World.dbug("IsComplete_PreReq: " .. work:ToString())
	if (work.inventoryItemQtyRemaining == 0) then
		return true
	else	
		return false
	end
	
end

function OnComplete_PreReq(work) 
	World.dbug("OnComplete_PreReq: " .. work:ToString())
	work.parent:RemovePrereq(work)
	

end
function IsReady_PreReq(work) 
	local total_qty = GetStockpileQty(work.inventoryItemName) + GetLooseQty(work.inventoryItemName)
	if (total_qty >= work.inventoryItemQtyRemaining) then
		return true
	else	
		return false
	end
end
-- ------------------------------------------------InstalledItem---------------
function OnWork_InstalledItem (work, deltaTime)
	--World.dbug("OnWork_InstalledItemWork: " .. work:ToString())
	if (work.complete) then
		return
	end
	coolDown = work.assignedRobot.info:GetFloat("PleaseMoveCoolDown",0)

	if (coolDown <= 0) then

		if (work.workTile.countOfOccupied == 0 or (work.workTile.countOfOccupied == 1 and work.workTile:IsItMe(work.assignedRobot))) then
			if (work.workTime > 0) then
				work.workTime = work.workTime - deltaTime

				if (work.workTime <= 0) then
					work:DoOnComplete()
				end
			else	
				work:DoOnComplete()
			end		
            

		else 
			coolDown = math.random(1,2)

			work.workTile:PleaseMove();
		end
	else
		coolDown = coolDown - deltaTime
	end
	if (work.assignedRobot ~= nil) then
		work.assignedRobot.info:SetFloat("PleaseMoveCoolDown",coolDown)
	end
	
end



function OnComplete_InstalledItem(work)
	--World.dbug("OnComplete_InstalledItemWork: " .. work:ToString())

	
	World.current:PlaceInstalledItem(work.installedItemProto.type, work.workTile)

end

function IsComplete_InstalledItem(work)
	--World.dbug("IsComplete_InstalledItemWork: " .. work:ToString())
	if work.workTime <= 0 then
		return true
	else
		return false
	end


end

function IsReady_InstalledItem(work)	
	--World.dbug("IsReady_InstalledItemWork: " .. work:ToString())
	if (work:GetCountOfPrereqs() > 0) then
		return false
	else
		return true
	end

end


---------------------------------------------------HAUL TO STOCKPILE--------------------

function SetHaul(work,invItemName,qty) 
	work.inventoryItemName = invItemName
	work.inventoryItemQtyRequired = qty
	work.inventoryItemQtyRemaining = qty
	work.inventorySearchStockpiles = false


	work.OnWork:Add("OnWork_Haul")
	work.OnComplete:Add("OnComplete_AnyPre")
	work.OnComplete:Add("OnComplete_Haul")
	work.OnComplete:Add("OnComplete_AnyPost")
	work.IsComplete = "IsComplete_Haul"
	work.IsReady = "IsReady_Haul"
	World.current.inventoryManager:SetStockPileSettingWork(invItemName, true)
end

function IsReady_Haul(work)
	return true

end

function IsComplete_Haul(work) 
	if (work.inventoryItemQtyRemaining == 0) then
		return true
	else
		return false
	end
end


function OnComplete_Haul(work)
	World.current.inventoryManager:SetStockPileSettingWork(work.inventoryItemName, false)
	work.workTile:RemoveWork(work)
end

function OnWork_Haul(work)
	work.assignedRobot:PlaceItemOnTile()
	
end

-----------------------------------------------WORK AT STATION -------------------------------------
function SetWorkstation(work, recipeName) 

	work.recipe = Recipe.GetRecipe(recipeName)
	work.workTime = work.recipe.buildTime
	work.canWorkFromNeighbourTiles = false
	for kvp in luanet.each(work.recipe.resources) do
		resource = kvp.Value
		rName = resource.name
		rQty = resource.qtyRequired
		--[[
		World.dbug(tostring(rName .. " " .. rQty))
		World.dbug(resource.name .. " " .. resource.qtyRequired)


		]]--
		--World.dbug("k=" .. resource.Key .. ", v=" .. resource.Value:ToString())

		newItem = WorkItem.MakeWorkItem(work.workTile,"SetPrereq",work,rName,rQty)
		newItem.description = "description (" .. rName  .. ":" .. rQty .. ")"

		work:AddPrereq(newItem)

	end
	work.OnWork:Add("OnWork_Workstation")
	work.OnComplete:Add("OnComplete_AnyPre")
	work.OnComplete:Add("OnComplete_Workstation")
	work.OnComplete:Add("OnComplete_AnyPost")
	work.IsComplete = "IsComplete_Workstation"
	work.IsReady = "IsReady_Workstation"
	work.description = "Workstation"

	--[[
	for (string resourceName in j.recipe.resources.Keys) {
        //(Tile tile, Action<Job> cbJobComplete, Action<Job> cbJobCancelled, JOB_TYPE type, string description, Recipe recipe, string name) {
        Job nj = Job.MakeRecipeJob(j.tile, JOB_TYPE.HAUL, JOB_TYPE.HAUL.ToString(), j.recipe, resourceName, j);
        nj.cbLuaRegisterJobComplete("JobQueue_HaulJobComplete");
        nj.cbLuaRegisterJobCancelled("JobQueue_HaulJobCancelled");
        Add(nj, nj.priority);
        //publicJobs.Add(nj);


        }

	--  ]]

	
end


function OnWork_Workstation(work,deltaTime) 
	OnWork_InstalledItem(work,deltaTime)
end

function OnComplete_Workstation(work)
	local products = work.recipe:GetProducts()
	
	for product in luanet.each(products) do 
		--local product = products.Current


		if (product.type == RECIPE_PRODUCT_TYPE.ENTITY) then
			local tile = World.current:FindEmptyUnnoccupiedTile(work.workTile)
			if (tile ~= nil) then
				local res = World.current:CreateEntityAtTile(tile, product.name)
				--World.dbug("create robot = " .. tostring(res))
			end


		elseif (product.type == RECIPE_PRODUCT_TYPE.INVENTORY_ITEM) then
			--how many	
			local qty = math.random(product.qtyMin, product.qtyMax)
			-- where
			local tile = World.current:FindTileForInventoryItem(work.workTile,product.name, qty);

			if (tile ~= nil) then
				tile:AddToInventory(product.name,qty)
			end
		elseif (product.type == RECIPE_PRODUCT_TYPE.MONEY) then
			amt = math.random() * (product.qtyMax - product.qtyMin) + product.qtyMin

			WorldController.Instance:AddCurrency(amt)
		

		end

	end



end

function IsComplete_Workstation(work)
	if work.workTime <= 0 then
		return true
	else
		return false
	end

end

function IsReady_Workstation(work)
	if (work:GetCountOfPrereqs() > 0) then
		return false
	else
		return true
	end
end






