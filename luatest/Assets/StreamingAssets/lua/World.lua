function GetStockpileQty(inventoryItemName) 
	return World.current.inventoryManager:GetStockpileQty(inventoryItemName)
end

function GetStockPileQty(inventoryItemName)
	return GetStockpileQty(inventoryItemName)
end

function GetLooseQty(inventoryItemName)
	return World.current.inventoryManager:GetLooseQty(inventoryItemName)

end

function GetQty(inventoryItemName) 
	return World.current.inventoryManager:GetStockpileQty(inventoryItemName)
end

function GetMaxQty(inventoryItemName)
	return World.current.inventoryManager:GetStockpileMaxQty(inventoryItemName)
end

function GetTotalQty(inventoryItemName)
	return GetQty(inventoryItemName) + GetLooseQty(inventoryItemName)
end

function CheckStockpiles() 
	

	local toCreate = nil
	local sps = World.current.inventoryManager.StockpileSettingsList:GetNext()
	local foundInv = nil
	if (sps ~= nil and sps.currentQty < sps.maxQty and not sps.pendingWork) then
		--World.dbug("checking for " .. sps.name .. " c/m=" .. sps.currentQty .. "/" .. sps.maxQty)

		if (sps.currentQty > 0 and sps.currentQty % sps.stackSize ~= 0) then
			local inventory = World.current.inventoryManager.stockpilesList:GetNext()
			if (inventory ~= nil and inventory.tile.HasPendingWork == false) then

				local tileQty = inventory:HowMany(sps.name)

				--World.dbug("\tchecking filled tile " .. inventory.tile.world_x .. "," .. inventory.tile.world_y .. " q=" .. tileQty)
				if (tileQty > 0 and tileQty < sps.stackSize) then
					foundInv = inventory

					local looseQty = World.current.inventoryManager:GetLooseQty(sps.name)
						
					local qtyWanted = sps.maxQty - sps.currentQty
					if (qtyWanted > sps.stackSize) then
						qtyWanted = sps.stackSize
					end

					if (tileQty + qtyWanted > sps.stackSize) then
						qtyWanted = sps.stackSize - tileQty
					end

					if (qtyWanted > looseQty) then
						qtyWanted = looseQty
					end
					if (qtyWanted > 0) then
						toCreate = {tile = inventory.tile, func = "SetHaul", name = sps.name, qty = qtyWanted}
						sps.pendingWork = true
						goto makework
					end					
				end
			end
			goto finish
		end
		
		

		--look for empty
		::checkforempty::
		do -- limits scope
			--local e_inventories = World.current.inventoryManager.stockpiles:GetEnumerator()
			--while e_inventories:MoveNext() do
			local e_inv = World.current.inventoryManager.stockpilesList:GetNext()

				

			if (e_inv ~= nil and e_inv:IsEmpty() and e_inv.tile.HasPendingWork == false) then
			

				local looseQty = World.current.inventoryManager:GetLooseQty(sps.name)

				--World.dbug("\tchecking empty tile " .. e_inv.tile.world_x .. "," .. e_inv.tile.world_y .. " loose="..looseQty)
						
				local qtyWanted = sps.maxQty - sps.currentQty
				if (qtyWanted > sps.stackSize) then
					qtyWanted = sps.stackSize
				end


				if (qtyWanted > looseQty) then
					qtyWanted = looseQty
				end
				if (qtyWanted > 0) then
					toCreate = {tile = e_inv.tile, func = "SetHaul", name = sps.name, qty = qtyWanted}
					sps.pendingWork = true
					goto makework
				end
			end
			
		end
	end

	--make work
	::makework::
	if (toCreate ~= nil) then
			
		WorkItem.MakeWorkItem(toCreate["tile"], toCreate["func"], toCreate["name"], toCreate["qty"])
		goto finish
	end
		
	::finish::

end
--[[

function CheckStockpiles() 
	
	local spsList = World.current.inventoryManager.stockpileSettings.Values:GetEnumerator()
	while spsList:MoveNext() do
		local toCreate = nil
		local sps = spsList.Current
		local foundInv = nil
		if (sps.currentQty < sps.maxQty and not sps.pendingWork) then
			World.dbug("checking for " .. sps.name .. " c/m=" .. sps.currentQty .. "/" .. sps.maxQty)
			local inventories = World.current.inventoryManager.stockpiles:GetEnumerator()
			while inventories:MoveNext() do
				local inventory = inventories.Current
				if (inventory.tile.HasPendingWork == false) then

					local tileQty = inventory:HowMany(sps.name)

					World.dbug("\tchecking filled tile " .. inventory.tile.world_x .. "," .. inventory.tile.world_y .. " q=" .. tileQty)
					if (tileQty > 0 and tileQty < sps.stackSize) then
						foundInv = inventory

						local looseQty = World.current.inventoryManager:GetLooseQty(sps.name)
						
						local qtyWanted = sps.maxQty - sps.currentQty
						if (qtyWanted > sps.stackSize) then
							qtyWanted = sps.stackSize
						end

						if (tileQty + qtyWanted > sps.stackSize) then
							qtyWanted = sps.stackSize - tileQty
						end

						if (qtyWanted > looseQty) then
							qtyWanted = looseQty
						end
						if (qtyWanted > 0) then
							toCreate = {tile = inventory.tile, func = "SetHaul", name = sps.name, qty = qtyWanted}
							sps.pendingWork = true
							goto makework
						end



					
					
					end
				end


			end
		

			--look for empty
			::checkforempty::
			do -- limits scope
				local e_inventories = World.current.inventoryManager.stockpiles:GetEnumerator()
				while e_inventories:MoveNext() do
					local e_inv = e_inventories.Current

				

					if (e_inv:IsEmpty() and e_inv.tile.HasPendingWork == false) then
			

						local looseQty = World.current.inventoryManager:GetLooseQty(sps.name)

						World.dbug("\tchecking empty tile " .. e_inv.tile.world_x .. "," .. e_inv.tile.world_y .. " loose="..looseQty)
						
						local qtyWanted = sps.maxQty - sps.currentQty
						if (qtyWanted > sps.stackSize) then
							qtyWanted = sps.stackSize
						end


						if (qtyWanted > looseQty) then
							qtyWanted = looseQty
						end
						if (qtyWanted > 0) then
							toCreate = {tile = e_inv.tile, func = "SetHaul", name = sps.name, qty = qtyWanted}
							sps.pendingWork = true
							goto makework
						end



					end
				end
			end
		end

		--make work
		::makework::
		if (toCreate ~= nil) then
			
			WorkItem.MakeWorkItem(toCreate["tile"], toCreate["func"], toCreate["name"], toCreate["qty"])
			goto finish
		end
		
	end
	::finish::

end

]]
--[[
csInventoryIndex = 0

function CheckStockpilesOld()

	
	local toCreate = {}

	local inventories = World.current.inventoryManager.stockpiles:GetEnumerator()
	--local totalInventories = 
	local currentinvIndex = -1
	while inventories:MoveNext() do
		currentinvIndex = currentinvIndex + 1
		if (currentinvIndex ~= csInventoryIndex) then
			goto nextInventory
		else
			
			--World.dbug("csInventoryIndex = " .. tostring(csInventoryIndex))
		end
		
		
		local inventory = inventories.Current
		if (inventory.IsStockPile) then 
			
			if (inventory.tile.HasPendingWork) then goto nextInventory end

			local spsList = World.current.inventoryManager.stockpileSettings.Values:GetEnumerator()

			while spsList:MoveNext() do
				local sps = spsList.Current
				if (sps.pendingWork == false and (inventory:TotalQty() == 0 or inventory:HowMany(sps.name) > 0)) then
					local tileQty = inventory:HowMany(sps.name)
					

					if (sps.currentQty < sps.maxQty and tileQty < sps.stackSize) then

						local looseQty = World.current.inventoryManager:GetLooseQty(sps.name)
						
						local qtyWanted = sps.maxQty - sps.currentQty
						if (qtyWanted > sps.stackSize) then
							qtyWanted = sps.stackSize
						end

						if (tileQty + qtyWanted > sps.stackSize) then
							qtyWanted = sps.stackSize - tileQty
						end

						if (qtyWanted > looseQty) then
							qtyWanted = looseQty
						end
						if (qtyWanted > 0) then
							toCreate[#toCreate+1] = {tile = inventory.tile, func = "SetHaul", name = sps.name, qty = qtyWanted}
							sps.pendingWork = true
							goto finish
						end
						--WorkItem.MakeWorkItem(inventory.tile,"SetHaul",sps.name,qtyWanted)


					end
				end

			end
		end
		::nextInventory::
	end
	::finish::

	for n = 1, #toCreate do
		a = toCreate[n]
		WorkItem.MakeWorkItem(a["tile"], a["func"], a["name"], a["qty"])

	end
	if (World.current.inventoryManager.stockpiles.Count > 0) then
		csInventoryIndex = (csInventoryIndex + 1 ) % World.current.inventoryManager.stockpiles.Count
	else	
		csInventoryIndex = 0
	end


end
]]