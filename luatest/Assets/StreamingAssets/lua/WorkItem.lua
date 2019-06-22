function SetInstalledItem(work,itemName) 
	

	proto = InstalledItem.GetPrototype(itemName)

	if (proto ~= nil) then
		work.installedItemProto = proto
		work.recipe = proto.recipe

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

			

		end
		work.OnWork:Add("OnWork_InstalledItemWork")
		work.OnComplete:Add("OnComplete_InstalledItemWork")
		work.IsComplete = "IsComplete_InstalledItemWork"
		work.IsReady = "IsReady_InstalledItemWork"

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

	work.OnWork:Add("OnWork_PreReq")
	work.OnComplete:Add("OnComplete_PreReq")
	work.IsComplete = "IsComplete_PreReq"
	work.IsReady = "IsReady_PreReq"
	   	 
end

function OnWork_PreReq(work)
	invItemName = work.inventoryItemName;
	robot = work.assignedRobot
	qtyNeeded = work.inventoryItemQtyRemaining

	qtyTaken = robot.inventory:RemoveFromInventory(invItemName.qtyNeeded)

	work.inventory.AddInventoryItem(invItemName, qtyTaken)
	work.inventoryItemQtyRemaining = work.inventoryItemQtyRemaining - qtyTaken
		
	if (work.inventoryItemQtyRemaining == 0) then
		work:DoOnComplete()
	end
end

function IsComplete_PreReq(work) 
	if (work.inventoryItemQtyRemaining == 0) then
		return true
	else	
		return false
	end
	
end

function OnComplete_PreReq(work) 
	work.parent.prereq:Remove(work)
	

end
function IsReady_PreReq(work) 
	return true
end

function OnWork_InstalledItemWork (work, deltaTime)
	if work.workTime > 0 then
		work.workTime = work.workTime - deltaTime

		if (work.workTime <= 0) then

			work:DoOnComplete()
		end
	end
end


-- ------------------------------------------------InstalledItem---------------
function OnComplete_InstalledItemWork(work)
	World.current:PlaceInstalledItem(work.installedItemProto.type, work.workTile)

end

function IsComplete_InstalledItemWork(work)
	if work.workTime <= 0 then
		return true
	else
		return false
	end


end

function IsReady_InstalledItemWork(work) 
	if work.prereq.Count > 0 then
		return false
	else
		return true
	end

end



