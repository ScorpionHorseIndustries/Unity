function GetStockpileQty(inventoryItemName) 
	return World.current.inventoryManager:GetStockpileQty(inventoryItemName)
end

function GetLooseQty(inventoryItemName)
	return World.current.inventoryManager:GetLooseQty(inventoryItemName)

end


function CheckStockpiles()
	local toCreate = {}
	local inventories = World.current.inventoryManager.inventories:GetEnumerator()

	while inventories:MoveNext() do
		
		local inventory = inventories.Current
		if (inventory.IsStockPile) then 
			
			if (inventory.tile.HasPendingWork) then goto nextInventory end

			local spsList = World.current.inventoryManager.stockpileSettings.Values:GetEnumerator()

			while spsList:MoveNext() do
				local sps = spsList.Current
				if ((inventory:TotalQty() == 0 or inventory:HowMany(sps.name) > 0) and sps.pendingWork == false) then
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
							goto nextInventory
						end
						--WorkItem.MakeWorkItem(inventory.tile,"SetHaul",sps.name,qtyWanted)


					end
				end

			end
		end
		::nextInventory::
	end

	for n = 1, #toCreate do
		a = toCreate[n]
		WorkItem.MakeWorkItem(a["tile"], a["func"], a["name"], a["qty"])

	end


end