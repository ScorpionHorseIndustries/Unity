

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
    tile = item:GetWorkSpot()


    if (tile.HasPendingJob == false) then
      j = Job.MakeStandardJob(
        tile,
        --Workstation_JobComplete,
        --Workstation_JobCancelled,
        JOB_TYPE.WORK_AT_STATION,
        1,
        item.workRecipeName

        )
	  j:cbLuaRegisterJobComplete("OnJobComplete_Workstation")
	  j:cbLuaRegisterJobCancelled("OnJobCancelled_Workstation")
      world.current.jobQueue:Push(j)
    end
end

function OnJobComplete_Workstation(job)
	InstalledItemActions.Workstation_JobComplete(job)
end

function  OnJobCancelled_Workstation(job)
	-- body
	InstalledItemActions.Workstation_JobCancelled(job)
end

function OnUpdate_Stockpile(item, deltaTime) 

	

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
	  ]]
    elseif (not item.tile:IsInventoryEmpty() and not item.tile.HasPendingWork) then

      itemName = item.tile:GetFirstInventoryItem()
      qtyRequired = InventoryItem.GetStackSize(itemName) - item.tile:InventoryTotal(itemName)
      nearest = World.current.inventoryManager:GetNearest(item.tile, itemName, false)
	  --[[
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
	  ]]
    end

end

function OnEnterRequested_Door(item) 

  item.itemParameters:SetBool("opening", true)

  if (item.itemParameters:GetFloat("openness") >= 1) then
    return Tile.CAN_ENTER.YES
  else 
    return Tile.CAN_ENTER.SOON
  end


end
