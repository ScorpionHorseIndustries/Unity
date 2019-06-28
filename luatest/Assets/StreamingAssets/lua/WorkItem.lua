


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
	

	proto = InstalledItem.GetPrototype(itemName)

	for x = work.workTile.world_x, proto.width, 1 do
		for y = work.workTile.world_y, proto.height, 1 do
			tile = World.current:GetTileAt(x,y)
			if (tile ~= nil) then
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
	return true
end
-- ------------------------------------------------InstalledItem---------------
function OnWork_InstalledItem (work, deltaTime)
	--World.dbug("OnWork_InstalledItemWork: " .. work:ToString())
	if (work.complete) then
		return
	end
	coolDown = work.assignedRobot.info:GetFloat("PleaseMoveCoolDown",0)

	if (coolDown <= 0) then

		if (work.workTile.countOfOccupied == 0 or (work.workTile.countOfOccupied == 1 and work.workTile:IsItMe(this))) then
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

			work.workTile:PleaseMove(work.assignedRobot);
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
	World.dbug("IsReady_InstalledItemWork: " .. work:ToString())
	if (work:GetCountOfPrereqs() > 0) then
		return false
	else
		return true
	end

end



