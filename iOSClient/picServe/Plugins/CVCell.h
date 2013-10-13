//
//  CVCell.h
//  picServe
//
//  Created by JED LIPPOLD on 8/3/13.
//
//

#import <UIKit/UIKit.h>

@interface CVCell : UICollectionViewCell
@property (nonatomic, strong) IBOutlet UILabel *titleLabel;
@property (nonatomic, strong) IBOutlet UIImageView *imageView;
@property (nonatomic, strong) IBOutlet UIButton *deleteButton;
@property (nonatomic, strong) IBOutlet UIImageView *videoOverlay;

@end

