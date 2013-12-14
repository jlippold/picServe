//
//  CVCell.m
//  picServe
//
//  Created by JED LIPPOLD on 8/3/13.
//
//

#import "CVCell.h"
#import "ImageCollection.h"

@implementation CVCell

- (id)initWithFrame:(CGRect)frame {
    
    self = [super initWithFrame:frame];
    
    
    

    
    if (self) {
        

        
        // Initialization code
        NSArray *arrayOfViews = [[NSBundle mainBundle] loadNibNamed:@"CView" owner:self options:nil];
        
        if ([arrayOfViews count] < 1) {
            return nil;
        }
        
        if (![[arrayOfViews objectAtIndex:0] isKindOfClass:[UICollectionViewCell class]]) {
            return nil;
        }
        
        self = [arrayOfViews objectAtIndex:0];
        

        
    }
    

    return self;
    
}




@end
