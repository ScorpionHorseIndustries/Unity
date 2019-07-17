
function SetSocialMediaName(item) 
	item.itemParameters:SetString("socialMediaName", World.current:GetSocialMediaName())

end

function OnUpdate_Dummy(item,deltaTime) 


end

function Clamp01 (f) 
	if (f < 0) then
		return 0
	elseif (f > 1) then
		return 1
	end

	return f
end

function OnUpdate_Plant(item, deltaTime) 

	
	local age = item.itemParameters:GetInt("age",0)
	age = age + 1
	local maxGrowthStage = item.itemParameters:GetInt("maxGrowthStage")
	local growthStage = item:GetGrowthStage()

	if (item.growthStage < maxGrowthStage) then

		if (age > growthStage.length) then
			item.growthStage = item.growthStage + 1
			age = 0
			if (item.cbOnChanged ~= nil) then
				item.cbOnChanged(item)
			end
		end
	end
	item.itemParameters:SetInt("age", age)
	
	

end

function OnUpdate_Door(item, deltaTime) 
	
	opentime = item.itemParameters:GetFloat("opentime", 0.25)

    if (item.itemParameters:GetBool("opening") == true) then
      item.itemParameters:IncFloat("openness", deltaTime / opentime)
    else 
      item.itemParameters:IncFloat("openness", -(deltaTime / opentime))
    end

    f = item.itemParameters:GetFloat("openness")
	f = Clamp01(f)
    
    item.itemParameters:SetFloat("openness", f)

    if (item.itemParameters:GetFloat("openness") >= 1) then
      item.itemParameters:SetBool("opening", false)
    end
    
    if (item.cbOnChanged ~= nil) then
      item.cbOnChanged(item)
    end

end

function OnUpdate_Workstation(item, deltaTime) 
	if (item.active == false) then return end

    local tile = item:GetWorkSpot()
	--World.dbug("item = " .. item.workRecipeName .. " " .. tile:ToString())
	

    if (tile.HasPendingWork == false) then
		if (item.canChangeRecipe and item.nextWorkRecipeName ~= item.workRecipeName and item.nextWorkRecipeName ~= nil) then
			item.workRecipeName = item.nextWorkRecipeName
		end

		if (item.workCondition ~= nil) then
			local wc = load("return ".. item.workCondition)
			if (not wc()) then goto skip end

		end
		WorkItem.MakeWorkItem(tile,"SetWorkstation",item.workRecipeName, "abc", "def");
    end
	::skip::
end


--[[
function OnJobComplete_Workstation(job)
	InstalledItemActions.Workstation_JobComplete(job)
end

function  OnJobCancelled_Workstation(job)
	-- body
	InstalledItemActions.Workstation_JobCancelled(job)
end
]]

function OnUpdate_Stockpile(item, deltaTime) 

	--[[

    if (item.tile:IsInventoryEmpty() and not item.tile.HasPendingWork) then
		
      randomItem = InventoryItem.GetRandomPrototype()
	  itemName = randomItem.type
	  World.dbug("hello " .. itemName)
	  --[[
      nearest = World.current.inventoryManager:GetNearest(item.tile, itemName, false)
      if (nearest == nil) then return end

      j = Job.MakeTileToTileJob(
        nearest,
          item.tile,
          itemName,
          InventoryItem.GetStackSize(itemName)
          )
      j:cbLuaRegisterJobComplete("JobQueue_HaulToTileComplete")
      j:cbLuaRegisterJobCancelled("JobQueue_HaulJobCancelled")
      j.cancelIfReturned = true

      World.current.jobQueue:Push(j)
	  
    elseif (not item.tile:IsInventoryEmpty() and not item.tile.HasPendingWork) then

      itemName = item.tile:GetFirstInventoryItem()
      qtyRequired = InventoryItem.GetStackSize(itemName) - item.tile:InventoryTotal(itemName)
      nearest = World.current.inventoryManager:GetNearest(item.tile, itemName, false)
	  
      if (nearest ~= nil) then
        qtyRequired = math.min(qtyRequired, nearest:InventoryTotal(itemName))

        if (qtyRequired > 0) then
          j = Job.MakeTileToTileJob(
            nearest,
            item.tile,
            itemName,
            qtyRequired

              )
          j.cancelIfReturned = true
          j:cbLuaRegisterJobComplete("JobQueue_HaulToTileComplete")
          j:cbLuaRegisterJobCancelled("JobQueue_HaulJobCancelled")

          World.current.jobQueue:Push(j)
        end
      end
	  
    end
	]]
end

function OnEnterRequested_Door(item) 

  item.itemParameters:SetBool("opening", true)

  if (item.itemParameters:GetFloat("openness") >= 1) then
    return Tile.CAN_ENTER.YES
  else 
    return Tile.CAN_ENTER.SOON
  end


end

function OnCreate_MiningController(item) 
	local tile = item.tile
	if (tile.type.minedProduct == nil) then
		item.workRecipeName = "";
		
	elseif (tile.type.minedProduct == "inv::coal") then
		item.workRecipeName = "recipe::installed::mining_controller::work::coal"
	elseif (tile.type.minedProduct == "inv::iron_ore") then
		item.workRecipeName = "recipe::installed::mining_controller::work::iron"
	elseif (tile.type.minedProduct == "inv::copper_ore") then
		item.workRecipeName = "recipe::installed::mining_controller::work::copper"
	end
	

end


function OnCreate_MiningSpot(item)

	local tile = item.tile
	if (tile.type.minedProduct == nil) then
		item.workRecipeName = "";
		
	elseif (tile.type.minedProduct == "inv::coal") then
		item.workRecipeName = "recipe::installed::mining_spot::work::coal"
	elseif (tile.type.minedProduct == "inv::iron_ore") then
		item.workRecipeName = "recipe::installed::mining_spot::work::iron"
	elseif (tile.type.minedProduct == "inv::copper_ore") then
		item.workRecipeName = "recipe::installed::mining_spot::work::copper"
	elseif (tile.type.minedProduct == "inv::sand") then
		item.workRecipeName = "recipe::installed::mining_spot::work::sand"
	elseif (tile.type.minedProduct == "inv::stone_slabs") then
		item.workRecipeName = "recipe::installed::mining_spot::work::stone"
	end
	


end